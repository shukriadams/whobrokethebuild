using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Web
{
    public class BuildLevelPluginHelper
    {
        public static void InvokeEvents(string eventCategory, IEnumerable<string> plugins, Build build)
        {
            ILogger logger = LogHelper.GetILogger<BuildLevelPluginHelper>();

            foreach (string plugin in plugins)
            {
                try
                {
                    IBuildLevelProcessor postProcessor = PluginProvider.GetByKey(plugin) as IBuildLevelProcessor;
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
