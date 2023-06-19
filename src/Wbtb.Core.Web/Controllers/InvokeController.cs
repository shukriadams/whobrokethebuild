using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class InvokeController : Controller
    {
        #region FIELDS

        private readonly Configuration _config;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        #endregion

        public InvokeController() 
        {
            _di = new SimpleDI();
            _config = _di.Resolve<Configuration>();
            _pluginProvider = _di.Resolve<PluginProvider>();  
        } 

        /// <summary>
        /// Puts a message in queue
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> Invoke()
        {
            Type pluginType = this.GetType();
            try
            {
                string json;
                using (StreamReader reader = new StreamReader(Request.Body))
                    json = await reader.ReadToEndAsync();

                PluginArgs pluginArgs = JsonConvert.DeserializeObject<PluginArgs>(json);
                IPlugin plugin = _pluginProvider.GetByKey(pluginArgs.pluginKey);
                pluginType = plugin.GetType();

                MethodInfo method = pluginType.GetMethod(pluginArgs.FunctionName);
                if (method == null)
                    throw new Exception ($"FunctionName {pluginArgs.FunctionName} not found");

                ArrayList methodArgs = new ArrayList();
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    PluginFunctionParameter incomingparameter = pluginArgs.Arguments.FirstOrDefault(r => r.Name == parameter.Name);
                    if (!parameter.IsOptional && incomingparameter == null)
                        throw new Exception($"Missing required parameter ${parameter.Name}");

                    if (incomingparameter != null)
                        methodArgs.Add(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(incomingparameter.Value), parameter.ParameterType));
                }

                plugin.ContextPluginConfig = _config.Plugins.Single(p => p.Key == pluginArgs.pluginKey);

                // note : we don't support async methods
                object result = method.Invoke(plugin, methodArgs.ToArray());
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return string.Join(string.Empty, PluginOutputEncoder.Encode(ex.ToString(), pluginType));
            }
        }
    }


}
