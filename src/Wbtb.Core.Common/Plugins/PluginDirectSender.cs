﻿using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// A plugin sender for use during dev, when plugins are run in the same application context as the core wbtb server.
    /// In normal production mode the PluginShellSender would be used instead.
    /// </summary>
    public class PluginDirectSender : IPluginSender
    {
        private readonly Configuration _config;

        private readonly MessageQueueHtppClient _messageQueueHtppClient;

        public PluginDirectSender() 
        {
            SimpleDI di = new SimpleDI();
            _config = di.Resolve<Configuration>();
            _messageQueueHtppClient = di.Resolve<MessageQueueHtppClient>();
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

            return this.Invoke<TReturnType>(
                args,
                "wbtb-invoke",
                pluginConfig.Manifest.Concrete);
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
                "wbtb-invoke",
                pluginConfig.Manifest.Concrete);
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
        private TReturnType Invoke<TReturnType>(object data, string switchName, string concreteType)
        {
            // write data to message queue

            string id = _messageQueueHtppClient.Add(data);
            string tid = Guid.NewGuid().ToString(); //todo store for cross check

            Type? concreteResolvedType = TypeHelper.ResolveType(concreteType);
            if (concreteResolvedType == null)
                throw new Exception($"Failed to resolved required concrete type {concreteResolvedType}");


            // OTHER SIDE OF WALL
            string messageData = _messageQueueHtppClient.Retrieve(id);
            PluginArgs pluginArgs = null;

            pluginArgs = JsonConvert.DeserializeObject<PluginArgs>(messageData);
            if (pluginArgs == null)
                throw new Exception($"Failed to deserialize json data");

            MethodInfo method = concreteResolvedType.GetMethod(pluginArgs.FunctionName);
            if (method == null)
                throw new Exception($"FunctionName {pluginArgs.FunctionName} not found");

            ArrayList methodArgs = new ArrayList();
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                PluginFunctionParameter incomingparameter = pluginArgs.Arguments.FirstOrDefault(r => r.Name == parameter.Name);
                if (!parameter.IsOptional && incomingparameter == null)
                    throw new Exception($"Missing required parameter ${parameter.Name}");

                if (incomingparameter != null)
                    methodArgs.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(incomingparameter.Value), parameter.ParameterType));
            }

            SimpleDI di = new SimpleDI();
            IPlugin pluginInstance = di.Resolve(concreteResolvedType) as IPlugin;
            pluginInstance.ContextPluginConfig = _config.Plugins.Single(p => p.Key == pluginArgs.pluginKey);

            object result = method.Invoke(pluginInstance, methodArgs.ToArray());
            string replyId = _messageQueueHtppClient.Add(result);

            // THIS SIDE OF WALL AGAIN
            string jsonOut = string.Join(string.Empty, PluginOutputEncoder.Encode<TReturnType>(replyId));
            Regex regex = new Regex(@"<WBTB-output(.*)>([\S\s]*?)<\/WBTB-output>");
            Match match = regex.Match(jsonOut);
            if (!match.Success)
                throw new ConfigurationException($"Command call failed, got unexpected output:{result}");

            if (typeof(TReturnType) == typeof(NullReturn))
                return default(TReturnType);

            string parsedReplyId = JsonConvert.DeserializeObject<string>(match.Groups[match.Groups.Count - 1].Value);
            if (parsedReplyId != replyId)
                throw new Exception($"parsedReplyId {parsedReplyId} mismatch with replyId {replyId}.");

            string payload = _messageQueueHtppClient.Retrieve(parsedReplyId);
            return JsonConvert.DeserializeObject<TReturnType>(payload);
        }
    }
}
