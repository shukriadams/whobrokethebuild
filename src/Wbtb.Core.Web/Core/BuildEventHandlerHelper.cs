using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildEventHandlerHelper
    {
        private readonly ILogger _logger;

        private readonly PluginProvider _pluginProvider;

        private readonly SimpleDI _di;

        public BuildEventHandlerHelper(PluginProvider pluginProvider, ILogger logger) 
        {
            _di = new SimpleDI();
            _pluginProvider = pluginProvider;
            _logger = logger;
        }

        public void InvokeEvents(string eventCategory, IEnumerable<string> eventHandlerPlugins, Build build)
        {
            foreach (string eventHandlerPlugin in eventHandlerPlugins)
            {
                try
                {
                    IBuildEventHandler handler = _pluginProvider.GetByKey(eventHandlerPlugin) as IBuildEventHandler;
                    handler.Process(build);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"failed {eventCategory} hook {eventHandlerPlugin } for {build.Id} : {ex}");
                }
            }
        }
    }
}
