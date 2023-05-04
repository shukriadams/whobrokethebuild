using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wbtb.Core.Common
{
    public class PluginShellSender : IPluginSender
    {
        #region FIELDS

        private readonly MessageQueueHtppClient _messageQueueHtppClient;

        private readonly Config _config;

        #endregion

        #region CTORS

        public PluginShellSender(MessageQueueHtppClient messageQueueHtppClient, Config config) 
        {
            _messageQueueHtppClient = messageQueueHtppClient;
            _config = config;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Common metho for calling all standard interface methods in plugin.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pluginName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public TReturnType InvokeMethod<TReturnType>(IPluginProxy callingProxy, PluginArgs args)
        {
            PluginConfig pluginConfig = _config.Plugins.SingleOrDefault(r => r.Key == callingProxy.PluginKey);
            if (pluginConfig == null)
                throw new ConfigurationException($"no config found for plugin id {callingProxy.PluginKey}. did plugin fail to load?");

            args.pluginKey = callingProxy.PluginKey;

            return this.Invoke<TReturnType>(
                args,
                pluginConfig.Path, 
                pluginConfig.Manifest.Runtime, 
                pluginConfig.Manifest.Main);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callingProxy"></param>
        /// <param name="args"></param>
        public void InvokeMethod(IPluginProxy callingProxy, PluginArgs args)
        {
            PluginConfig pluginConfig = _config.Plugins.SingleOrDefault(r => r.Key == callingProxy.PluginKey);
            if (pluginConfig == null)
                throw new ConfigurationException($"no config found for plugin id {callingProxy.PluginKey}. did plugin fail to load?");

            args.pluginKey = callingProxy.PluginKey;

            this.Invoke<NullReturn>(
                args,
                pluginConfig.Path,
                pluginConfig.Manifest.Runtime,
                pluginConfig.Manifest.Main);
        }

        /// <summary>
        /// Calls a plugin with a single --<switchname> arg, and optional data packet of any object. object MUST be serializable to JSON.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="switchName"></param>
        /// <param name="workingDirectory"></param>
        /// <param name="runtime"></param>
        /// <param name="mainBin"></param>
        /// <returns></returns>
        private TReturnType Invoke<TReturnType>(object data, string workingDirectory, string runtime, string mainBin)
        {
            Shell shell = new Shell
            {
                // use pluginName to resolve this
                WorkingDirectory = workingDirectory,
                WriteToConsole = true
            };

            // write data to message queue
            string id = _messageQueueHtppClient.Add(data);
            string tid = Guid.NewGuid().ToString(); //todo store for cross check

            string command = $"{runtime} {mainBin} --wbtb-message {id} --wbtb-tid {tid}";
            string result;

            try
            {
                if (_config.LogOutgoingProxyCalls)
                {
                    PluginLogger.Write($"Plugin invocation: {command}");
                }

                result = shell.Run(command);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Regex regex = new Regex(@"<WBTB-output(.*)>([\S\s]*?)<\/WBTB-output>");
            Match match = regex.Match(result);
            if (!match.Success)
                throw new ConfigurationException($"Command call failed, got unexpected output:{result}");

            if (typeof(TReturnType) == typeof(NullReturn))
                return default(TReturnType);

            string json = match.Groups[match.Groups.Count - 1].Value;
            try
            {
                return JsonConvert.DeserializeObject<TReturnType>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse plugin result to expected type {typeof(TReturnType)}");
                Console.WriteLine(json);
                Console.WriteLine(result, ex);

                throw ex;
            }
        }

        #endregion
    }
}
