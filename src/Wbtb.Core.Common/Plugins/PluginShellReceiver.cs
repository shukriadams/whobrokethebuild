﻿using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Plugin-side handler for incoming console messages. This class is run in plugins only, and only when the plugin runs in standalone mode. "Under development" plugins
    /// in a Visual Studio solution with the core will be called directly, bypassing this receiver.
    /// </summary>
    /// <typeparam name="TPlugin"></typeparam>
    public class PluginShellReceiver<TPlugin>
    {
        #region METHODS

        private void ProcessInterfaceCommand(string interfaceData, string[] args, Configuration config, TPlugin pluginInstance)
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

                MethodInfo method = typeof(TPlugin).GetMethod(pluginArgs.FunctionName);
                if (method == null)
                {
                    PluginLogger.Write($"FunctionName {pluginArgs.FunctionName} not found");
                    Environment.Exit(1);
                    return;
                }

                if (pluginArgs.Arguments == null)
                    pluginArgs.Arguments = new PluginFunctionParameter[0];

                ArrayList methodArgs = new ArrayList();
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    PluginFunctionParameter incomingparameter = pluginArgs.Arguments.FirstOrDefault(r => r.Name == parameter.Name);
                    if (!parameter.IsOptional && incomingparameter == null)
                    {
                        PluginLogger.Write($"Missing required parameter \"{parameter.Name}\" on method call \"{pluginArgs.FunctionName}\".");
                        Environment.Exit(1);
                        return;
                    }

                    if (incomingparameter != null)
                        methodArgs.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(incomingparameter.Value), parameter.ParameterType));
                }

                ((IPlugin)pluginInstance).ContextPluginConfig = config.Plugins.Single(p => p.Key == pluginArgs.pluginKey);

                // note : we don't support async methods
                object result = method.Invoke(pluginInstance, methodArgs.ToArray());

                SimpleDI di = new SimpleDI();
                MessageQueueHtppClient client = di.Resolve<MessageQueueHtppClient>();
                string messageid = client.Add(result);

                PrintJSONToSTDOut(PluginOutputEncoder.Encode<TPlugin>(messageid));
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
                ConsoleHelper.WriteLine(line);

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
            // do global setup, this is the application entry-point for plugins running in standalone mode
            SimpleDI di = new SimpleDI();

            di.Register<MessageQueueHtppClient, MessageQueueHtppClient>();
            di.Register<ConfigurationBasic, ConfigurationBasic>();
            di.Register<PluginDirectSender, PluginDirectSender>();
            di.Register<PluginCoreSender, PluginCoreSender>();
            di.Register<PersistPathHelper, PersistPathHelper>();
            di.Register<PluginProvider, PluginProvider>();
            di.Register<IPluginSender, PluginCoreSender>();

            CommandLineSwitches switches = new CommandLineSwitches(args);

            MessageQueueHtppClient client = di.Resolve<MessageQueueHtppClient>();
            
            // fetch config from messenger service
            Configuration config = client.GetConfig();
            if (config == null)
                throw new Exception("Failed to get config from MessageQueue. Has config been initialized?");
                
            config.IsCurrentContextProxyPlugin = true;
            ConfigurationBasic configBasic = di.Resolve<ConfigurationBasic>();
            di.RegisterSingleton<Configuration>(config);
            
            // register this plugin as the type defined in this assembly
            di.Register(typeof(TPlugin), typeof(TPlugin));

            // register other plugin types as proxies
            string thisPluginName = TypeHelper.Name<TPlugin>();
            foreach (PluginConfig pluginConfig in config.Plugins.Where(p => p.Manifest.Concrete != thisPluginName))
                di.Register(TypeHelper.ResolveType(pluginConfig.Manifest.Interface), TypeHelper.GetRequiredProxyType(pluginConfig.Manifest.Interface));

            // retrieve message for actual data
            if (switches.Contains("wbtb-message"))
            {
                string messageid = switches.Get("wbtb-message");
                string data = client.Retrieve(messageid);

                if (configBasic.PersistCalls)
                    File.WriteAllText("__interfaceCall.txt", data);

                TPlugin pluginInstance = di.Resolve<TPlugin>();
                ProcessInterfaceCommand(data, args, config, pluginInstance);
            }

            if (switches.Contains("manifest"))
            {
                string manifestPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Wbtb.yml");
                
                if (File.Exists(manifestPath))
                { 
                    string[] manifest = File.ReadAllLines(manifestPath);
                    foreach(string line in manifest)
                        ConsoleHelper.WriteLine(line);
                }

                return;
            }

            if (switches.Contains("help")) 
            {
                string status = "WBTB plugin catcher - no or invalid args specified. Args are : \n" +
                    "--manifest to view manifest\n" +
                    "--diagnose to test plugin\n" +
                    "--wbtb-message <messageid>";

                PrintJSONToSTDOut(PluginOutputEncoder.Encode<TPlugin>(status));
            }

            // default to diagnose mode
            IPlugin plugin = di.Resolve<TPlugin>() as IPlugin;
            plugin.Diagnose();
        }

        #endregion
    }
}
