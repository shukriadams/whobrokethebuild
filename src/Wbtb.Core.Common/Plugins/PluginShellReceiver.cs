using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Wbtb.Core.Common.Plugins
{
    /// <summary>
    /// Plugin-side handler for incoming console messages. This class is run in plugins only, and only when the plugin runs in standalone mode. "Under development" plugins
    /// in a Visual Studio solution with the core will be called directly, bypassing this receiver.
    /// </summary>
    /// <typeparam name="TPlugin"></typeparam>
    public class PluginShellReceiver<TPlugin>
    {
        #region METHODS

        private static string DecodeJsonString(string jsonString)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(jsonString));
        }

        private void ProcessInterfaceCommand(string interfaceData, string[] args)
        {
            try 
            {
                PluginArgs pluginArgs = null;

                try
                {
                    pluginArgs = JsonConvert.DeserializeObject<PluginArgs>(interfaceData);
                    if (pluginArgs == null)
                        throw new Exception($"Failed to deserialize json data");
                }
                catch (Exception ex)
                {
                    // todo handle this better
                    PluginLogger.WriteError($"Failed to parse wbtbArgs : ", ex);
                    PluginLogger.Write(interfaceData);
                    return;
                }

                // register this plugin. 
                // todo : this is messy, move to calling method with other DI registration
                SimpleDI di = new SimpleDI();
                Config config = di.Resolve<Config>();
                di.Register(typeof(TPlugin), typeof(TPlugin));


                MethodInfo method = typeof(TPlugin).GetMethod(pluginArgs.FunctionName);
                if (method == null)
                {
                    PluginLogger.Write($"FunctionName {pluginArgs.FunctionName} not found");
                    Environment.Exit(1);
                    return;
                }

                ArrayList methodArgs = new ArrayList();
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    PluginFunctionParameter incomingparameter = pluginArgs.Arguments.FirstOrDefault(r => r.Name == parameter.Name);
                    if (!parameter.IsOptional && incomingparameter == null)
                    {
                        PluginLogger.Write($"Missing required parameter ${parameter.Name}");
                        Environment.Exit(1);
                        return;
                    }

                    if (incomingparameter != null)
                        methodArgs.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(incomingparameter.Value), parameter.ParameterType));
                }

                TPlugin pluginInstance = di.Resolve<TPlugin>();
                ((IPlugin)pluginInstance).ContextPluginConfig = config.Plugins.Single(p => p.Key == pluginArgs.pluginKey);
                Console.WriteLine($"Invoking method {pluginArgs.FunctionName}");

                // note : we don't support async methods
                object result = method.Invoke(pluginInstance, methodArgs.ToArray());
                PrintJSONToSTDOut(PluginOutputEncoder.Encode<TPlugin>(result));
            }
            catch (Exception ex)
            { 
                PluginLogger.WriteError("Unexpected error", ex);
                PluginLogger.Write($"Invocation data: {interfaceData}");
                PluginLogger.Write($"Invoke directly with: {string.Join(" ", args)}");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="obj"></param>
        private static void PrintJSONToSTDOut(IEnumerable<string> data)
        {
            foreach(string line in data)
                Console.WriteLine(line);

            Environment.Exit(0);
        }

        /// <summary>
        /// Accepts :
        /// --function function/method name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        public void Process(string[] args)
        {
            CommandLineSwitches switches = new CommandLineSwitches(args);

            MessageQueueHtppClient client = new MessageQueueHtppClient();

            Config config = client.GetConfig();
            config.IsCurrentContextProxyPlugin = true;
            SimpleDI di = new SimpleDI();
            di.RegisterSingleton<Config>(config);
            // register proxy types, except for plugin this is being currently used from, that will always be concrete
            string thisPluginName = TypeHelper.Name<TPlugin>();
            foreach (PluginConfig pluginConfig in config.Plugins.Where(p => p.Manifest.Concrete != thisPluginName))
                di.Register(TypeHelper.ResolveType(pluginConfig.Manifest.Interface), TypeHelper.GetRequiredProxyType(pluginConfig.Manifest.Interface));

                // initialize plugin, this must be done once at app start, after handshake
            if (switches.Contains("wbtb-initialize"))
            {
                // todo : set session id for security fingerprint

                if (ConfigBasic.Instance.PersistCalls)
                    File.AppendAllText("__init.txt", switches.Get("wbtb-initialize"));
                

                PluginInitResult initResult = new PluginInitResult { SessionId = Guid.NewGuid().ToString() };

                // todo : replace with dedicated init result type
                PrintJSONToSTDOut(PluginOutputEncoder.Encode<TPlugin>(initResult));
                return;
            }

            // invoke method directly
            if (switches.Contains("wbtb-invoke"))
            {
                if (ConfigBasic.Instance.PersistCalls)
                    File.WriteAllText("__interfaceCall.txt", DecodeJsonString(switches.Get("wbtb-invoke")));

                ProcessInterfaceCommand(DecodeJsonString(switches.Get("wbtb-invoke")), args);
            }

            // invoke method using arguments stored in text file, this is for dev/debugging
            if (switches.Contains("wbtb-invokecached"))
            {
                if (!File.Exists(switches.Get("wbtb-invokecached")))
                    throw new ConfigurationException($"File ${switches.Get("wbtb-invokecached")} not found");

                ProcessInterfaceCommand(File.ReadAllText(switches.Get("wbtb-invokecached")), args);
            }

            // retrieve message for actual data
            if (switches.Contains("wbtb-message"))
            {
                string messageid = switches.Get("wbtb-message");
                string data = client.Retrieve(messageid);



                if (ConfigBasic.Instance.PersistCalls)
                    File.WriteAllText("__interfaceCall.txt", data);

                ProcessInterfaceCommand(data, args);
            }

            if (switches.Contains("manifest"))
            {
                string manifestPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Wbtb.yml");
                
                if (File.Exists(manifestPath))
                { 
                    string[] manifest = File.ReadAllLines(manifestPath);
                    foreach(string line in manifest)
                        Console.WriteLine(line);
                }

                return;
            }

            string status = "WBTB plugin catcher - no or invalid args specified. Args are : \n" +
                "--manifest to view manifest\n" +
                "--wbtb-initialize to view manifest\n" +
                "--wbtb-invoke <methodname>";

            PrintJSONToSTDOut(PluginOutputEncoder.Encode<TPlugin>(status));
        }

        #endregion
    }
}
