using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using Group = Wbtb.Core.Common.Group;

namespace Wbtb.Core
{
    public class ConfigurationLoader
    {
        #region METHODS

        /// <summary>
        /// Required first step for working with static config - set's the path config 
        /// </summary>
        /// <param name="path"></param>
        public Configuration LoadUnvalidatedConfig(string path, bool verbose)
        {
            if (string.IsNullOrEmpty(path))
                throw new ConfigurationException("Config path cannot be an empty string");

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                throw new ConfigurationException($"Could not resolve directory in path {path}");

            if (!File.Exists(path))
                throw new ConfigurationException($"Config path was set to \"{path}\", but no config file was found at this location. You can override path with the \"{Constants.ENV_VAR_CONFIG_PATH}\" env var.");

            string rawYml = File.ReadAllText(path);

            if (verbose)
                ConsoleHelper.WriteLine($"WBTB : Loaded config @ {path}", addDate: false);

            // substitute templated {...} settings from env variables. this is a security feature so we can store sensitive data as 
            // env vars instead of in config file
            MatchCollection evnVarTokens = new Regex("\\{\\{env.(.*?)\\}\\}", RegexOptions.Multiline).Matches(rawYml);
            foreach (Match evnVarToken in evnVarTokens)
            {
                string envVarValue = Environment.GetEnvironmentVariable(evnVarToken.Groups[1].Value);
                if (envVarValue == null)
                {
                    throw new ConfigurationException($"Config has a template value {{env." + evnVarToken.Groups[1].Value + "}}, but no matching env var value was found.");
                }
                else
                {
                    if (verbose)
                        ConsoleHelper.WriteLine($"Injecting environment variable for \"{evnVarToken.Groups[1].Value}\".", addDate: false);
                    rawYml = rawYml.Replace("{{env." + evnVarToken.Groups[1].Value + "}}", envVarValue);
                }
            }

            IDeserializer deserializer = YmlHelper.GetDeserializer();
            if (verbose)
                ConsoleHelper.WriteLine("WBTB : initializing config", addDate: false);

            ConfigurationValidationError validation = ValidateYmlParsing(rawYml);
            if (!validation.IsValid)
                throw new ConfigurationException($"Application config yml at path {path} is not properly formatted. See WBTB setup guide for details. {validation.Message}");

            // load raw yml config into strongly-typed object structure used internally by WBTB
            Configuration tempConfig = deserializer.Deserialize<Configuration>(rawYml);

            // load raw yml config into dynamic structure, this will be used to break up and parse fragments of config to specific plugins as JSON. This allows 
            // plugins to define their own config structure without requiring updates to WBTB's internal config structure.
            YamlNode rawConfig = ConfigurationHelper.RawConfigToDynamic(rawYml);

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
                    job.Message = job.Message.Where(a => a.Enable).ToList();
            }

            // find and embed raw config data as JSON onto plugins, groups and users. Raw JSON allows plugins to define custom
            // attributes that are not covered by strong typing. This data can then be JSON deserialized at the plugin and used
            // there. This is a compromise to give maximum flexibility with plugin config
            foreach (PluginConfig pluginConfig in tempConfig.Plugins)
                pluginConfig.RawJson = ConfigurationHelper.GetRawPluginConfigByPluginId(rawConfig, pluginConfig.Key);

            foreach (Group group in tempConfig.Groups)
                for (int i = 0; i < group.Message.Count; i++)
                    group.Message[i].RawJson = ConfigurationHelper.GetRawAlertConfigByIndex(rawConfig, "Groups", group.Key, i);

            foreach (User user in tempConfig.Users)
                for (int i = 0; i < user.Message.Count; i++)
                    user.Message[i].RawJson = ConfigurationHelper.GetRawAlertConfigByIndex(rawConfig, "Users", user.Key, i);

            AutofillOptionalValues(tempConfig);

            tempConfig.Hash = Sha256.FromString(rawYml);

            return tempConfig;
        }

        public void ConfigValidated(Configuration unsafeConfig)
        {
            SimpleDI di = new SimpleDI();
            di.RegisterSingleton<Configuration>(unsafeConfig);
        }

        /// <summary>
        /// move this to independent class, no need to be here
        /// </summary>
        /// <param name="config"></param>
        public bool FetchPlugins(Configuration config)
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
                catch (Exception ex)
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
        public void EnsureNoneManifestLogicValid(Configuration config)
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

            Regex remindFormat = new Regex(@"\d+", RegexOptions.IgnoreCase);

            foreach (BuildServer buildserver in config.BuildServers)
                foreach (Job job in buildserver.Jobs)
                    foreach (MessageHandler messageHandler in job.Message)
                    {
                        // ensure remind format valid
                        if (!string.IsNullOrEmpty(messageHandler.Remind))
                        {
                            Match match = remindFormat.Match(messageHandler.Remind);
                            if (!match.Success)
                                throw new ConfigurationException($"Job \"{job.Name}\" has a message handler with an invalid remind value \"{messageHandler.Remind}\". Value must be an integer, and is always in hours.");
                        }
                    }
        }

        /// <summary>
        /// Tries to load manifest data from plugins. Also validates manifests and plugin, and fails app start if necessary.
        /// </summary>
        /// <param name="config"></param>
        public void LoadManifestData(Configuration config, bool verbose)
        {
            CurrentVersion currentVersion = new CurrentVersion();
            currentVersion.Resolve();

            // enforce API version check if semver of current version is not all zero (local dev)
            bool doAPIVersionCheck = currentVersion.CoreVersion.Major > 0 && currentVersion.CoreVersion.Minor > 0 && currentVersion.CoreVersion.Patch > 0;
            if (!doAPIVersionCheck && verbose)
                ConsoleHelper.WriteLine("Skipping semver checking, dev versioning detected", addDate: false);

            foreach (PluginConfig pluginConfig in config.Plugins)
            {
                if (string.IsNullOrEmpty(pluginConfig.Path))
                    throw new ConfigurationException($"ERROR : Plugin \"{pluginConfig.Key}\" does not declare Path. Path should be absolute path to plugin script/binary, or for internal plugins, should be plugin's Namespace, egs \"Wbtb.Extensions.Data.Postgres.\"");

                // try to autoresolve plugin location, this is a dev feature. normally path will point to an absolute location, but as a dev aid
                // we allow it be set to a project name in-solution. If we can find plugin files at a location in solution, we automatically use
                // those files' location as plugin path
                bool directoryExists = Directory.Exists(pluginConfig.Path);
                bool isExternal = directoryExists;

                if (!directoryExists)
                {
                    // try to autoresolve if running in visual studio
                    // web project starts in the project root
                    string devPath = Path.Combine($"..{Path.DirectorySeparatorChar}", pluginConfig.Path, "bin", "Debug", "net6.0");
                    // cli starts in bin/debug/runtime
                    if (!Directory.Exists(devPath))
                        devPath = Path.Combine($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}", pluginConfig.Path, "bin", "Debug", "net6.0");

                    if (Directory.Exists(devPath))
                    {
                        if (verbose)
                            ConsoleHelper.WriteLine($"plugin location automatically remapped from {pluginConfig.Path} to {devPath}", addDate : false);

                        pluginConfig.Path = devPath;
                        pluginConfig.Proxy = false;
                        isExternal = true;
                    }
                }

                if (!Directory.Exists(pluginConfig.Path))
                    throw new ConfigurationException($"ERROR : Plugin \"{pluginConfig.Key}\"'s Path \"{pluginConfig.Path}\" could not be resolved.");

                // try to get manifest directly from file path, this is for local dev  mainly, but _should_ work on production(?)
                string pluginManifestRaw = null;
                if (isExternal)
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

                    pluginManifestRaw = ResourceHelper.ReadResourceAsString(pluginAssembly, "Wbtb.yml");
                }

                // load and parse manifest from YML
                try
                {
                    IDeserializer deserializer = YmlHelper.GetDeserializer();
                    pluginConfig.Manifest = deserializer.Deserialize<PluginManifest>(pluginManifestRaw);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException($"Failed to deserialize manifest yml for plugin {pluginConfig.Key} : {ex}");
                }

                // ensure that plugin manifest loaded, past here here we can assume manifest always exists
                if (pluginConfig.Manifest == null)
                    throw new ConfigurationException($"Failed to load manifest for plugin {pluginConfig.Key}. Manifest does not exist or is malformed.");

                // ensure that plugins's API version is patch version compatible 
                if (doAPIVersionCheck)
                {
                    SemanticVersion pluginVersion = SemanticVersion.TryParse(pluginConfig.Manifest.APIVersion);
                    if (pluginVersion.Major != currentVersion.CoreVersion.Major || pluginVersion.Minor != currentVersion.CoreVersion.Minor || pluginVersion.Patch > currentVersion.CoreVersion.Patch)
                        throw new ConfigurationException($"Plugin {pluginConfig.Key} at version {pluginConfig.Manifest.Version} has an APIVersion requirement {pluginConfig.Manifest.APIVersion} that is not compatible with this Wbtb core version {currentVersion.CoreVersion}.");
                }

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
            IEnumerable<PluginConfig> dataLayers = config.Plugins.Where(p => p.Manifest.Interface == TypeHelper.Name(typeof(IDataPlugin)));
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
                foreach (Job job in buildserver.Jobs)
                {

                    // ensure job alerts are associated with valid plugins, and that plugins validate alert config
                    foreach (MessageHandler alert in job.Message)
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
                foreach (MessageConfiguration alertConfig in user.Message)
                {
                    if (string.IsNullOrEmpty(alertConfig.Plugin))
                        throw new ConfigurationException($"User \"{user.Key}\" defines an Alert that has no \"Plugin\" property.");

                    PluginConfig pluginConfig = config.Plugins.FirstOrDefault(p => p.Key == alertConfig.Plugin);
                    if (pluginConfig == null)
                        throw new ConfigurationException($"User \"{user.Key}\" defines an Alert with plugin \"{alertConfig.Plugin}\", but this plugin does not exist.");

                    if (pluginConfig.Manifest.Interface != TypeHelper.Name(typeof(IMessagingPlugin)))
                        throw new ConfigurationException($"User \"{user.Key}\" defines an Alert with plugin \"{alertConfig.Plugin}\", but this plugin enables interface \"{pluginConfig.Manifest.Interface}\" instead of \"{TypeHelper.Name(typeof(IMessagingPlugin))}\".");
                }
            }

            // todo : merge with user logic block above using interface?
            foreach (Group group in config.Groups)
                foreach (MessageConfiguration alertConfig in group.Message)
                {
                    if (string.IsNullOrEmpty(alertConfig.Plugin))
                        throw new ConfigurationException($"Group \"{group.Key}\" defines an Alert that has no \"Plugin\" property.");

                    PluginConfig pluginConfig = config.Plugins.FirstOrDefault(p => p.Key == alertConfig.Plugin);
                    if (pluginConfig == null)
                        throw new ConfigurationException($"Group \"{group.Key}\" defines an Alert with plugin \"{alertConfig.Plugin}\", but this plugin does not exist.");


                    if (pluginConfig.Manifest.Interface != TypeHelper.Name<IMessagingPlugin>())
                        throw new ConfigurationException($"Group \"{group.Key}\" defines an Alert with plugin \"{alertConfig.Plugin}\", but this plugin enables interface \"{pluginConfig.Manifest.Interface}\" instead of \"{TypeHelper.Name<IMessagingPlugin>()}\".");
                }


            foreach (SourceServer sourceServer in config.SourceServers)
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
                    // ensure that job's source server exists
                    // todo : how to ensure plugin found is of type source server?
                    if (!string.IsNullOrEmpty(jobConfig.SourceServer) && !config.SourceServers.Any(r => r.Key == jobConfig.SourceServer))
                        throw new ConfigurationException($"The SourceControlPlugin \"{jobConfig.SourceServer}\" in job \"{jobConfig.Key}\" is not defined as a plugin, or is not enabled.");

                    // if job name isn't set, revert to key as name
                    if (string.IsNullOrEmpty(jobConfig.Name))
                        jobConfig.Name = jobConfig.Key;

                    // ensure each logparsers exist as a plugins, and implement correct interface
                    foreach (string logparserPluginKey in jobConfig.LogParsers)
                    {
                        PluginConfig logParserPlugin = config.Plugins.FirstOrDefault(r => r.Key == logparserPluginKey);
                        if (logParserPlugin == null)
                            throw new ConfigurationException($"Job \"{jobConfig.Key}\" defines a log parser \"{logparserPluginKey}\", but no enabled plugin for this parser was found.");

                        Type pluginInterfaceType = TypeHelper.GetCommonType(logParserPlugin.Manifest.Interface);
                        if (!typeof(ILogParserPlugin).IsAssignableFrom(pluginInterfaceType))
                            throw new ConfigurationException($"Job \"{jobConfig.Key}\" defines a log parser \"{logparserPluginKey}\", but this plugin does not implement type {TypeHelper.Name<ILogParserPlugin>()}.");
                    }

                    // ensure postprocessors exist as a plugins, and implement correct interface
                    foreach (string postProcessorKey in jobConfig.PostProcessors)
                    {
                        PluginConfig postProcessorPlugin = config.Plugins.FirstOrDefault(r => r.Key == postProcessorKey);
                        if (postProcessorPlugin == null)
                            throw new ConfigurationException($"Job \"{jobConfig.Key}\" defines a postProcessor plugin \"{postProcessorKey}\", but no enabled plugin for this parser was found.");

                        Type pluginInterfaceType = TypeHelper.GetCommonType(postProcessorPlugin.Manifest.Interface);
                        if (!typeof(IPostProcessorPlugin).IsAssignableFrom(pluginInterfaceType))
                            throw new ConfigurationException($"Job \"{jobConfig.Key}\" defines a postProcessor plugin \"{postProcessorKey}\", but this plugin does not implement type {TypeHelper.Name<IPostProcessorPlugin>()}.");
                    }

                    // ensure event handlers impliment correct interfaces
                    ValidateProcessors<IBuildEventHandler>(config, jobConfig, jobConfig.OnBuildStart, "OnBuildStart");
                    ValidateProcessors<IBuildEventHandler>(config, jobConfig, jobConfig.OnBuildEnd, "OnBuildEnd");
                    ValidateProcessors<IBuildEventHandler>(config, jobConfig, jobConfig.OnBroken, "OnBroken");
                    ValidateProcessors<IBuildEventHandler>(config, jobConfig, jobConfig.OnFixed, "OnFixed");
                    ValidateProcessors<IBuildEventHandler>(config, jobConfig, jobConfig.OnLogAvailable, "OnLogAvailable");
                }
            }
        }

        private void ValidateProcessors<T>(Configuration config, Job job, IEnumerable<string> processors, string category)
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
        private void AutofillOptionalValues(Configuration config)
        {
            foreach (User user in config.Users)
            {
                if (string.IsNullOrEmpty(user.Name))
                    user.Name = user.Key;

                // take first three chars of name if intials not set.
                if (string.IsNullOrEmpty(user.Initials))
                {
                    user.Initials = user.Name.ToUpper().Replace(" ", string.Empty);
                    if (user.Initials.Length > 3)
                        user.Initials = user.Initials.Substring(0, 3);
                }

            }

            foreach (Group group in config.Groups)
                if (string.IsNullOrEmpty(group.Name))
                    group.Name = group.Key;

            foreach (BuildServer buildServer in config.BuildServers)
            {
                if (string.IsNullOrEmpty(buildServer.Name))
                    buildServer.Name = buildServer.Key;

                foreach (Job job in buildServer.Jobs)
                    if (string.IsNullOrEmpty(job.Name))
                        job.Name = job.Key;
            }

            foreach (SourceServer sourceServer in config.SourceServers)
                if (string.IsNullOrEmpty(sourceServer.Name))
                    sourceServer.Name = sourceServer.Key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="collectionName"></param>
        private void EnsureIdPresentAndUnique(IEnumerable<IIdentifiable> items, string collectionName)
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
        private ConfigurationValidationError ValidateYmlParsing(string ymlText)
        {
            IDeserializer deserializer = YmlHelper.GetDeserializer();

            try
            {
                deserializer.Deserialize<Configuration>(ymlText);
                return new ConfigurationValidationError { IsValid = true };
            }
            catch (Exception ex)
            {
                return new ConfigurationValidationError
                {
                    IsValid = false,
                    Message = ex.Message,
                    InnerException = ex
                };
            }
        }

        #endregion
    }
}