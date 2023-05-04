using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace Wbtb.Core.Common.Plugins
{
    /// <summary>
    /// plugin sender used from plugins. Calls are routed back via the core app, which can then service the call internally or route out to another plugin.
    /// </summary>
    public class PluginCoreSender : IPluginSender
    {
        private readonly Config _config;

        public PluginCoreSender(Config config) 
        {
            _config = config;
        }

        /// <summary>
        /// Special method for to pass config etc from main app to plugin. this is called after main app config is done. CAnnot be called at handshake time
        /// because at that point we don't yet know which apps are working. Initialize make all plugins aware of all other plugins, so all handshaking must 
        /// already be done at this point.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="workingDirectory"></param>
        /// <param name="runtime"></param>
        /// <param name="mainBin"></param>
        /// <returns></returns>
        public PluginInitResult Initialize(string pluginName)
        {
            throw new NotImplementedException("Not used in this context");
        }

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

            return this.Invoke<TReturnType>(args);
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

            this.Invoke<NullReturn>(args);
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
        private TReturnType Invoke<TReturnType>(object data)
        {
            WebClient client = new WebClient();
            string json = JsonConvert.SerializeObject(data);
            byte[] postData = Encoding.ASCII.GetBytes(json);
            byte[] reply = client.UploadData($"http://localhost:{_config.Port}/api/v1/invoke", postData);
            string result = Encoding.ASCII.GetString(reply);

            Regex regex = new Regex(@"<WBTB-output(.*)>([\S\s]*?)<\/WBTB-output>");
            Match match = regex.Match(result);
            if (!match.Success)
                throw new ConfigurationException($"Command call failed, got unexpected output:{result}");

            if (typeof(TReturnType) == typeof(NullReturn))
                return default(TReturnType);

            string innerJson = match.Groups[match.Groups.Count - 1].Value;
            try
            {
                
                return JsonConvert.DeserializeObject<TReturnType>(innerJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse plugin result {innerJson} to type {typeof(TReturnType).Name}.");
                Console.WriteLine(result, ex);

                throw ex;
            }
        }
    }
}
