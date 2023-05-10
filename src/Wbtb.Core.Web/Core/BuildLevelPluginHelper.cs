using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildLevelPluginHelper
    {
        private readonly ILogger _logger;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        public BuildLevelPluginHelper(PluginProvider pluginProvider, ILogger logger) 
        {
            _di = new SimpleDI();
            _pluginProvider = pluginProvider;
            _logger = logger;
        }

        public void InvokeEvents(string eventCategory, IEnumerable<string> plugins, Build build)
        {
            foreach (string plugin in plugins)
            {
                try
                {
                    IBuildLevelProcessor postProcessor = _pluginProvider.GetByKey(plugin) as IBuildLevelProcessor;
                    postProcessor.Process(build);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"failed {eventCategory} hook {plugin } for {build.Id} : {ex}");
                }
            }
        }
    }
}
