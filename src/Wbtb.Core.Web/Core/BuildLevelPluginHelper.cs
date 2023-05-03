using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Web
{
    public class BuildLevelPluginHelper
    {
        private readonly PluginProvider _pluginProvider;
        public BuildLevelPluginHelper() 
        {
            SimpleDI di = new SimpleDI();
            _pluginProvider = di.Resolve<PluginProvider>();
        }

        public void InvokeEvents(string eventCategory, IEnumerable<string> plugins, Build build)
        {
            ILogger logger = LogHelper.GetILogger<BuildLevelPluginHelper>();

            foreach (string plugin in plugins)
            {
                try
                {
                    IBuildLevelProcessor postProcessor = _pluginProvider.GetByKey(plugin) as IBuildLevelProcessor;
                    postProcessor.Process(build);
                }
                catch (Exception ex)
                {
                    logger.LogInformation($"failed {eventCategory} hook {plugin } for {build.Id} : {ex}");
                }
            }
        }
    }
}
