using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class BuildEventHandlerHelper
    {
        private readonly Logger _logger;

        private readonly PluginProvider _pluginProvider;

        public BuildEventHandlerHelper(PluginProvider pluginProvider, Logger logger) 
        {
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
                    _logger.Error(this, $"failed {eventCategory} hook {eventHandlerPlugin } for {build.Id}", ex);
                }
            }
        }
    }
}
