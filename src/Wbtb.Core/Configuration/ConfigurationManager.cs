using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Wbtb.Core.Common;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Wbtb.Core
{
    public class ConfigurationManager
    {
        #region PROPERTIES

        public static IEnumerable<string> AllowedInternalPlugins { get; set; }

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        static ConfigurationManager() 
        {
            AllowedInternalPlugins = new string[] { };
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Required first step for working with static config - set's the path config 
        /// </summary>
        /// <param name="path"></param>
        public static Config LoadUnsafeConfig(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ConfigurationException("Config path cannot be an empty string");

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                throw new ConfigurationException($"Could not resolve directory in path {path}");

            if (!File.Exists(path))
                throw new ConfigurationException($"Config path was set to \"{path}\", but no config file was found at this location.");

            string rawYml = File.ReadAllText(path);
            
            Console.WriteLine($"WBTB : Loaded config @ {path}");

            IDeserializer deserializer = YmlHelper.GetDeserializer();
            Console.WriteLine("WBTB : initializing config");

            ConfigValidationError validation = ValidateYmlFormat(rawYml);
            if (!validation.IsValid)
                throw new ConfigurationException($"Application config yml is not properly formatted. See WBTB setup guide for details. {validation.Message}");

            // load raw yml config into strongly-typed object structure used internally by WBTB
            Config tempConfig = deserializer.Deserialize<Config>(rawYml);

            // load raw yml config into dynamic structure, this will be used to break up and parse fragments of config to specific plugins as JSON. This allows 
            // plugins to define their own config structure without requiring updates to WBTB's internal config structure.
            YamlNode rawConfig = ConfigurationHelper.RawConfigToDynamic(rawYml);

            // substitute templated {...} settings from env variables. this is a security feature so we can store sensitive data as 
            // env vars instead of in config file
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^\{\{env.(.*)\}\}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            foreach (PluginConfig pluginConfig in tempConfig.Plugins)
            {
                IList<KeyValuePair<string, object>> writeableConfig = pluginConfig.Config.ToList();

                for (int i = 0; i < writeableConfig.Count; i++)
                {
                    KeyValuePair<string, object> item = pluginConfig.Config.ElementAt(i);
                    string value = item.Value.ToString().Trim();
                    System.Text.RegularExpressions.Match match = reg.Match(value);
                    if (!match.Success)
                        continue;

                    string envArgName = match.Groups[1].Value;
                    string envVarValue = Environment.GetEnvironmentVariable(envArgName);
                    if (string.IsNullOrEmpty(envVarValue))
                        throw new ConfigurationException($"plugin {pluginConfig.Key} has a config item {item.Key} requesting an env variable that is not set");

                    writeableConfig[i] = new KeyValuePair<string, object>(item.Key, envVarValue);
                    Console.WriteLine($"{pluginConfig.Key} config item {item.Key} value has been set from environment variable");
                }

                pluginConfig.Config = writeableConfig;
            }

            // removes explicitly disabled data as quickly as possible, this way we don't have to constantly filter out plugins by enabled in following checks
            tempConfig.Plugins = tempConfig.Plugins.Where(p => p.Enable).ToList();
            tempConfig.Groups = tempConfig.Groups.Where(g => g.Enable).ToList();
            tempConfig.BuildServers = tempConfig.BuildServers.Where(b => b.Enable).ToList();
            tempConfig.SourceServers = tempConfig.SourceServers.Where(s => s.Enable).ToList();
            tempConfig.Users = tempConfig.Users.Where(p => p.Enable).ToList();

            foreach (BuildServer buildServer in tempConfig.BuildServers)
            {
                buildServer.Jobs = buildServer.Jobs.Where(j => j.Enable).ToList();

                foreach (Job job in buildServer.Jobs)
                    job.Alert = job.Alert.Where(a => a.Enable).ToList();
            }

            // find and embed raw config data as JSON onto plugins, groups and users. Raw JSON allows plugins to define custom
            // attributes that are not covered by strong typing. This data can then be JSON deserialized at the plugin and used
            // there. This is a compromise to give maximum flexibility with plugin config
            foreach (PluginConfig pluginConfig in tempConfig.Plugins)
                pluginConfig.RawJson = ConfigurationHelper.GetRawPluginConfigByPluginId(rawConfig, pluginConfig.Key);

            foreach (Group group in tempConfig.Groups)
                for (int i = 0; i < group.Alert.Count; i++)
                    group.Alert[i].RawJson = ConfigurationHelper.GetRawAlertConfigByIndex(rawConfig, "Groups", group.Key, i);

            foreach (User user in tempConfig.Users)
                for (int i = 0; i < user.Alert.Count; i++)
                    user.Alert[i].RawJson = ConfigurationHelper.GetRawAlertConfigByIndex(rawConfig, "Users", user.Key, i);

            AutofillOptionalValues(tempConfig);

            EnsureNoneManifestLogicValid(tempConfig);

            return tempConfig;
        }
        
        public static void FinalizeConfig(Config unsafeConfig)
        {
            EnsureManifestLogicValid(unsafeConfig);
            SimpleDI di = new SimpleDI();
            di.RegisterSingleton<Config>(unsafeConfig);
        }


        /// <summary>
        /// move this to independent class, no need to be ehre
        /// </summary>
        /// <param name="config"></param>
        public static bool FetchPlugins(Config config)
        {
            // ensure write permission
            bool updated = false;
            SimpleDI di = new SimpleDI();
            GitHelper gitHelper = di.Resolve<GitHelper>();
            FileSystemHelper filesystemHelper = di.Resolve<FileSystemHelper>();

            foreach (PluginConfig pluginConfig in config.Plugins.Where(p => p.SourceType != PluginSourceTypes.None))
            {
                try
                {
                    Directory.CreateDirectory(pluginConfig.Path);
                }
                catch (Exception ex)
                { 
                    throw new ConfigurationException($"Failed to create plugin directory for {pluginConfig.Key}:{ex}");
                }
                
                try 
                {
                    string writeTest = Path.Combine(pluginConfig.Path, ".wbtb-writeTst");
                    File.WriteAllText(writeTest, string.Empty);
                    File.Delete(writeTest);
                }
                catch(Exception ex)
                {
                    throw new ConfigurationException($"Failed write test to {pluginConfig.Key}:{ex}");
                }
            }

            foreach (PluginConfig pluginConfig in config.Plugins.Where(plugin => plugin.SourceType == PluginSourceTypes.Git))
            {
                string fsSafePluginName = Convert.ToBase64String(Encoding.UTF8.GetBytes(pluginConfig.Key));
                string plugingWorkingDir = Path.Combine(config.PluginsWorkingDirectory, fsSafePluginName);
                Directory.CreateDirectory(plugingWorkingDir);
                string checkoutPath = Path.Combine(plugingWorkingDir, ".checkout");

                string tag = gitHelper.GetLatestTag(pluginConfig.Source, checkoutPath);
                string tagFile = Path.Combine(plugingWorkingDir, ".wbtbtag");

                if (string.IsNullOrEmpty(tag))
                    throw new ConfigurationException($"Could not get a tag from checkout for plugin {pluginConfig.Key}. Is repo tagged?");

                bool copy = true;
                if (File.Exists(tagFile))
                { 
                    string deployedTag = File.ReadAllText(tagFile);
                    if (deployedTag == tagFile)
                        copy = false;
                }

                if (copy)
                {
                    filesystemHelper.ClearDirectory(pluginConfig.Path);
                    filesystemHelper.CopyDirectory(checkoutPath, pluginConfig.Path);
                    File.WriteAllText(tagFile, tag);
                    updated = true;
                }
            }

            return updated;
        }

        /// <summary>
        /// Valids config not dependent on manifest data. Manifest data requires plugin fetching, which in turn requires that some base 
        /// config needs to be in place first.
        /// </summary>
        /// <param name="config"></param>
        private static void EnsureNoneManifestLogicValid(Config config)
        {
            EnsureIdPresentAndUnique(config.Plugins, "plugin");
            EnsureIdPresentAndUnique(config.BuildServers, "build server");
            EnsureIdPresentAndUnique(config.SourceServers, "source server");
            EnsureIdPresentAndUnique(config.Users, "user");
            EnsureIdPresentAndUnique(config.Groups, "group");

            if (string.IsNullOrEmpty(config.BuildLogsDirectory))
                throw new ConfigurationException("BuildLogsDirectory is empty");

            if (config.PagesPerPageGroup < 1)
                throw new ConfigurationException("PagesPerPageGroup cannot be less than 1");

        }

        /// <summary>
        /// Runs Config through a gauntlet to look for errors in data structure. On any error throws a ConfigurationException and aborts 
        /// application start. Does not change config structure
        /// </summary>
        /// <param name="config"></param>
        private static void EnsureManifestLogicValid(Config config)
        {
            foreach (PluginConfig pluginConfig in config.Plugins)
            {
                if (string.IsNullOrEmpty(pluginConfig.Path))
                    throw new ConfigurationException($"ERROR : Plugin \"{pluginConfig.Key}\" does not declare Path. Path should be absolute path to plugin script/binary, or for internal plugins, should be plugin's Namespace, egs \"Wbtb.Extensions.Data.Postgres.\"");

                // if path does not point to an existing location on disk, determie if path is a valid internal plugin
                if (Directory.Exists(pluginConfig.Path))
                {
                    pluginConfig.IsExternal = true;
                }
                else
                {
                    if (!ConfigurationManager.AllowedInternalPlugins.Contains(pluginConfig.Path))
                        throw new ConfigurationException($"ERROR : Plugin \"{pluginConfig.Key}\"'s Path \"{pluginConfig.Path}\" should be a directory path or an internal plugin in list \"{string.Join(", ", ConfigurationManager.AllowedInternalPlugins)}\".");
                }

                // try to get manifest directly from file path, this is for local dev  mainly, but _should_ work on production(?)
                string pluginManifestRaw = null;
                if (pluginConfig.IsExternal)
                {
                    string pluginYmlManifestPath = Path.Join(pluginConfig.Path, "Wbtb.yml");
                    if (!File.Exists(pluginYmlManifestPath))
                        throw new ConfigurationException($"ERROR : plugin \"{pluginConfig.Key}\" does not have a manifest @ expected location {pluginYmlManifestPath}.");

                    pluginManifestRaw = File.ReadAllText(pluginYmlManifestPath);
                }
                else 
                {
                    Assembly pluginAssembly = TypeHelper.GetAssembly(pluginConfig.Path);
                    if (pluginAssembly == null)
                        throw new ConfigurationException($"ERROR : plugin \"{pluginConfig.Key}\" is assumed running from assembly {pluginConfig.Path}, but no assembly with this name could be found.");

                    if (!ResourceHelper.ResourceExists(pluginAssembly, "Wbtb.yml"))
                        throw new ConfigurationException($"ERROR : plugin \"{pluginConfig.Key}\" does not have a manifest");

                    pluginConfig.Proxy = false;
                    pluginManifestRaw = ResourceHelper.ReadResourceAsString(pluginAssembly, "Wbtb.yml");
                }

                try
                {
                    IDeserializer deserializer = YmlHelper.GetDeserializer();
                    pluginConfig.Manifest = deserializer.Deserialize<PluginManifest>(pluginManifestRaw);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException($"Failed to deserialize manifest yml for plugin {pluginConfig.Key} : {ex}");
                }

                // ensure that plugin manifest exists, past here here we can assume manifest always exists
                if (pluginConfig.Manifest == null)
                    throw new ConfigurationException($"Failed to load manifest for plugin {pluginConfig.Key}. Manifest does not exist or is malformed.");

                if (string.IsNullOrEmpty(pluginConfig.Manifest.Runtime))
                    throw new ConfigurationException($"manifest yml for plugin {pluginConfig.Key} has no \"Runtime\" property.");

                // get plugin runtime, this determines how we validate the rest of manifest content
                object runtime = null;
                if (!Enum.TryParse(typeof(Runtimes), pluginConfig.Manifest.Runtime, true, out runtime))
                    throw new ConfigurationException($"manifest yml for plugin {pluginConfig.Key} has an unsupported runtime \"{pluginConfig.Manifest.Runtime}\".");
                pluginConfig.Manifest.RuntimeParsed = (Runtimes)runtime;

                // ensure that manifest's id is unique
                if (config.Plugins.Where(p => p.Manifest != null && p.Manifest.Key == pluginConfig.Manifest.Key).Count() > 1)
                    throw new ConfigurationException($"{pluginConfig.Key} imports a manifest with Id \"{pluginConfig.Manifest.Key}\", but this Id is used by at least one other plugin.");

                if (string.IsNullOrEmpty(pluginConfig.Manifest.Main))
                    throw new ConfigurationException($"manifest yml for plugin {pluginConfig.Key} has no \"Main\" property.");

                // ensure that plugin path combined with manifest main point to a file that exists
                if (pluginConfig.Proxy)
                {
                    string mainAbsolutePath = Path.Combine(pluginConfig.Path, pluginConfig.Manifest.Main);
                    if (!File.Exists(mainAbsolutePath))
                        throw new ConfigurationException($"plugin {pluginConfig.Key} defines a main file {pluginConfig.Manifest.Main} that was not found at expected path \"{mainAbsolutePath}\".");
                }

                if (string.IsNullOrEmpty(pluginConfig.Manifest.Interface))
                    throw new ConfigurationException($"manifest yml for plugin {pluginConfig.Key} has no \"Interface\" property.");

                if (pluginConfig.Manifest.RuntimeParsed == Runtimes.dotnet)
                {
                    if (string.IsNullOrEmpty(pluginConfig.Manifest.Assembly))
                        throw new ConfigurationException($"manifest yml for plugin {pluginConfig.Key} has no \"Assembly\" property.");

                    if (string.IsNullOrEmpty(pluginConfig.Manifest.Concrete))
                        throw new ConfigurationException($"manifest yml for plugin {pluginConfig.Key} has no \"Concrete\" property.");
                }

                // load types defined in manifest
                Type pluginInterface = TypeHelper.GetCommonType(pluginConfig.Manifest.Interface);
                if (pluginInterface == null)
                    throw new ConfigurationException($"ERROR : Config Plugin \"{pluginConfig.Key}\" defines an interface \"{pluginConfig.Manifest.Interface}\" that cannot be found.");

                // ensure that interface is allowed
                if (!typeof(IPlugin).IsAssignableFrom(pluginInterface))
                    throw new ConfigurationException($"ERROR : Plugin {pluginConfig.Key} defines an interface \"{pluginConfig.Manifest.Interface}\" That does not derive from IPlugin. Plugin will be disabled.");

                // capture proxy type from interface
                object[] attributes = pluginInterface.GetCustomAttributes(typeof(PluginProxyAttribute), true);
                if (attributes.Length == 0)
                    throw new ConfigurationException($"Inteface {pluginConfig.Manifest.Interface} missing PluginProxy attribute");

                PluginProxyAttribute attribute = attributes.First() as PluginProxyAttribute;
                pluginConfig.ForcedProxyType = $"{attribute.ProxyType.Namespace}.{attribute.ProxyType.Name}";
            }

            // confirm a datalayer plugin is active
            IEnumerable<PluginConfig> dataLayers = config.Plugins.Where(p => p.Manifest.Interface == TypeHelper.Name(typeof(IDataLayerPlugin)) );
            if (dataLayers.Count() == 0)
                throw new ConfigurationException("No datalayer plugin defined or enabled");

            if (dataLayers.Count() > 1)
                throw new ConfigurationException("Multiple datalayer plugins found - only one can be enabled.");

            // confirm group users exist
            foreach (Group group in config.Groups)
                foreach (string user in group.Users)
                    if (!config.Users.Any(u => u.Key == user))
                        throw new ConfigurationException($"Group \"{group.Key}\" defines a user \"{user}\" that is not also defined under \"Users\".");

            foreach (BuildServer buildserver in config.BuildServers)
                foreach (Job job in buildserver.Jobs){

                    // ensure job alerts are associated with valid plugins, and that plugins validate alert config
                    foreach (AlertHandler alert in job.Alert)
                        if (!config.Plugins.Any(g => g.Key == alert.Plugin))
                            throw new ConfigurationException($"Job \"{job.Key}\" defines an alert with plugin target \"{alert.Plugin}\", but this plugin does not exist.");
                }

            // ensure user plugin identities point to valid plugins 
            foreach (User user in config.Users)
            {
                // ensure that authplugin for user is set, and exists
                if (!string.IsNullOrEmpty(user.AuthPlugin))
                {
                    // ensure that authplugin exists
                    if (!config.Plugins.Any(p => p.Key == user.AuthPlugin))
                        throw new ConfigurationException($"User \"{user.Key}\" defines an AuthPlugin \"{user.AuthPlugin}\" that either does not exist, or is disabled.");
                }

                // ensure source server identities point to valid soure servers
                foreach (UserSourceIdentity userIdentity in user.SourceServerIdentities)
                {
                    if (string.IsNullOrEmpty(userIdentity.Name))
                        throw new ConfigurationException($"User \"{user.Key}\" defines a SourceServerIdentity that has no \"Name\" property.");

                    if (string.IsNullOrEmpty(userIdentity.SourceServerKey))
                        throw new ConfigurationException($"User \"{user.Key}\" defines a SourceServerIdentity that has no \"SourceServerKey\" property.");

                    if (!config.SourceServers.Any(p => p.Key == userIdentity.SourceServerKey))
                        throw new ConfigurationException($"User \"{user.Key}\" defines SourceServer \"{userIdentity.SourceServerKey}\", but this SourceServer does not exist.");
                }

                // ensure contact identities point to valid source servers
                foreach (AlertConfig alertConfig in user.Alert)
                {
                    if (string.IsNullOrEmpty(alertConfig.Key))
                        throw new ConfigurationException($"User \"{user.Key}\" defines an Alert that has no \"Id\" property.");

                    if (string.IsNullOrEmpty(alertConfig.Plugin))
                        throw new ConfigurationException($"User \"{user.Key}\" defines an Alert that has no \"Plugin\" property.");

                    PluginConfig pluginConfig = config.Plugins.FirstOrDefault(p => p.Key == alertConfig.Plugin);
                    if (pluginConfig == null)
                        throw new ConfigurationException($"User \"{user.Key}\" defines an Alert with plugin \"{alertConfig.Plugin}\", but this plugin does not exist.");

                    if (pluginConfig.Manifest.Interface != TypeHelper.Name(typeof(IMessaging)))
                        throw new ConfigurationException($"User \"{user.Key}\" defines an Alert with plugin \"{alertConfig.Plugin}\", but this plugin enables interface \"{pluginConfig.Manifest.Interface}\" instead of \"{TypeHelper.Name(typeof(IMessaging))}\".");
                }
            }

            // todo : merge with user logic block above using interface?
            foreach (Group group in config.Groups)
                foreach (AlertConfig alertConfig in group.Alert)
                {
                    if (string.IsNullOrEmpty(alertConfig.Key))
                        throw new ConfigurationException($"Group \"{group.Key}\" defines an alert that has no \"Key\" property.");

                    if (string.IsNullOrEmpty(alertConfig.Plugin))
                        throw new ConfigurationException($"Group \"{group.Key}\" defines an Alert that has no \"Plugin\" property.");

                    PluginConfig pluginConfig = config.Plugins.FirstOrDefault(p => p.Key == alertConfig.Plugin);
                    if (pluginConfig == null)
                        throw new ConfigurationException($"Group \"{group.Key}\" defines an Alert with plugin \"{alertConfig.Plugin}\", but this plugin does not exist.");


                    if (pluginConfig.Manifest.Interface != TypeHelper.Name<IMessaging>())
                        throw new ConfigurationException($"Group \"{group.Key}\" defines an Alert with plugin \"{alertConfig.Plugin}\", but this plugin enables interface \"{pluginConfig.Manifest.Interface}\" instead of \"{TypeHelper.Name<IMessaging>()}\".");
                }


            foreach(SourceServer sourceServer in config.SourceServers)
            {
                if (string.IsNullOrEmpty(sourceServer.Plugin))
                    throw new ConfigurationException($"Sourceserver \"{sourceServer.Key}\" has no Plugin property - a sourceseerver must point to a plugin to communicate with a source control system.");

                // ensure that source server plugin exists
                if (!config.Plugins.Any(r => r.Key == sourceServer.Plugin))
                    throw new ConfigurationException($"The plugin \"{sourceServer.Plugin}\" defined by source server \"{sourceServer.Key}\" is not defined as a plugin, or is not enabled.");
            }

            foreach (BuildServer buildServer in config.BuildServers)
            {
                EnsureIdPresentAndUnique(buildServer.Jobs, "job");

                if (string.IsNullOrEmpty(buildServer.Plugin))
                    throw new ConfigurationException($"Buildserver \"{buildServer.Key}\" has no Plugin property - a buildserver must point to a plugin to communicate with a machine.");

                // ensure that job's source server plugin exists
                if (!config.Plugins.Any(r => r.Key == buildServer.Plugin))
                    throw new ConfigurationException($"The buildserver \"{buildServer.Plugin}\" defined in \"{buildServer.Key}\" is not defined as a plugin, or is not enabled.");

                foreach (Job jobConfig in buildServer.Jobs)
                {
                    // ensure that job defines a source server 
                    if (string.IsNullOrEmpty(jobConfig.SourceServer))
                        throw new ConfigurationException($"Job \"{jobConfig.Key}\" has no SourceServer property - a job must point to a sourceserver plugin that its build content uses.");

                    // ensure that job's source server exists
                    // todo : how to ensure plugin found is of type source server?
                    if (!config.SourceServers.Any(r => r.Key == jobConfig.SourceServer))
                        throw new ConfigurationException($"The SourceControlPlugin \"{jobConfig.SourceServer}\" in job \"{jobConfig.Key}\" is not defined as a plugin, or is not enabled.");

                    // if job name isn't set, revert to key as name
                    if (string.IsNullOrEmpty(jobConfig.Name))
                        jobConfig.Name = jobConfig.Key;

                    // ensure each log parser exists as a plugin, and that plugin implements required interface
                    foreach(string logparserPluginKey in jobConfig.LogParserPlugins)
                    {
                        PluginConfig logParserPlugin = config.Plugins.FirstOrDefault(r => r.Key == logparserPluginKey);
                        Type pluginInterfaceType = TypeHelper.GetCommonType(logParserPlugin.Manifest.Interface);
                        if (logParserPlugin == null)
                            throw new ConfigurationException($"Job \"{jobConfig.Key}\" defines a log parser \"{logparserPluginKey}\", but no enabled plugin for this parser was found.");

                        if (!typeof(ILogParser).IsAssignableFrom(pluginInterfaceType))
                            throw new ConfigurationException($"Job \"{jobConfig.Key}\" defines a log parser \"{logparserPluginKey}\", but this plugin does not implement type {TypeHelper.Name<ILogParser>()}.");
                    }

                    // ensure each post processor implements required interface 
                    ValidateProcessors<IBuildLevelProcessor>(config, jobConfig, jobConfig.OnBuildStart, "OnBuildStart");
                    ValidateProcessors<IBuildLevelProcessor>(config, jobConfig, jobConfig.OnBuildEnd, "OnBuildEnd");
                    ValidateProcessors<IBuildLevelProcessor>(config, jobConfig, jobConfig.OnBroken, "OnBroken");
                    ValidateProcessors<IBuildLevelProcessor>(config, jobConfig, jobConfig.OnFixed, "OnFixed");
                    ValidateProcessors<IBuildLevelProcessor>(config, jobConfig, jobConfig.OnLogAvailable, "OnLogAvailable");
                }
            }
        }

        private static void ValidateProcessors<T>(Config config, Job job, IEnumerable<string> processors, string category) 
        {
            foreach (string processorPlugin in processors)
            {
                PluginConfig postProcessorPlugin = config.Plugins.FirstOrDefault(r => r.Key == processorPlugin);
                Type pluginInterfaceType = TypeHelper.GetCommonType(postProcessorPlugin.Manifest.Interface);
                if (postProcessorPlugin == null)
                    throw new ConfigurationException($"Job \"{job.Key}\" defines an {category} processor plugin \"{processorPlugin}\", but no enabled plugin with this key was found.");

                if (!typeof(T).IsAssignableFrom(pluginInterfaceType))
                    throw new ConfigurationException($"Job \"{job.Key}\" defines an {category} processor plugin \"{processorPlugin}\", but this plugin does not implement type {TypeHelper.Name<T>()}.");
            }
        }

        /// <summary>
        /// Where possible fills out values which can be inferred. This changes the contents of the config object.
        /// </summary>
        /// <param name="config"></param>
        private static void AutofillOptionalValues(Config config)
        { 
            foreach (User user in config.Users)
            { 
                if (string.IsNullOrEmpty(user.Name))
                    user.Name = user.Key;
            }

            foreach(BuildServer buildServer in config.BuildServers.Where(b => !string.IsNullOrEmpty(b.Url)))
            { 
                // ensure url protocol, this is optional in config
                if (!buildServer.Url.ToLower().StartsWith("http"))
                    buildServer.Url = $"http://{buildServer.Url}";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="collectionName"></param>
        private static void EnsureIdPresentAndUnique(IEnumerable<IIdentifiable> items, string collectionName)
        {
            foreach (IIdentifiable item in items)
            {
                // ensure that ids have values
                if (string.IsNullOrEmpty(item.Key))
                    throw new ConfigurationException($"ERROR : config yml contains a {collectionName} with no Key.");

                // ensure unique
                if (items.Where(r => r.Key == item.Key).Count() > 1)
                    throw new ConfigurationException($"ERROR : config yml contains more than one {collectionName} with Key {item.Key} - Key must be unique");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ymlText"></param>
        /// <returns></returns>
        private static ConfigValidationError ValidateYmlFormat(string ymlText)
        {
            IDeserializer deserializer = YmlHelper.GetDeserializer();

            try
            {
                deserializer.Deserialize<Config>(ymlText);
                return new ConfigValidationError { IsValid = true};
            }
            catch(Exception ex) 
            { 
                return new ConfigValidationError 
                { 
                    IsValid = false,
                    Message = ex.Message,
                    InnerException = ex
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetBlank()
        {
            Config testconfig = new Config();
            ISerializer serializer = new SerializerBuilder()
                .Build();

            return serializer.Serialize(testconfig);
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static PluginManifest ParseManifest(string rawYml)
        {
            IDeserializer deserializer = YmlHelper.GetDeserializer();
            return deserializer.Deserialize<PluginManifest>(rawYml);
        }

        #endregion
    }
}