using Microsoft.Extensions.Logging;
using Wbtb.Core.Common;
using System;
using System.Text;

namespace Wbtb.Core.Web.Core
{
    public class MetricsHelper
    {
        #region FIELDS

        private readonly PluginProvider _pluginProvider;

        private readonly ILogger _logger;

        #endregion

        #region CTORS

        public MetricsHelper(PluginProvider pluginProvider, ILogger logger) 
        {
            _pluginProvider = pluginProvider;
            _logger = logger;
        }

        #endregion

        #region METHODS

        public string GetInflux() 
        {
            try
            {
                StringBuilder s = new StringBuilder();
                s.AppendLine($"##whobrokethebuild:generated:{DateTime.UtcNow}##");
                IDataPlugin dataLayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();
                int buildErrors = dataLayer.GetFailingDaemonTasksCount();

                s.AppendLine($"whobrokethebuild build_errors_count={buildErrors}u");
                _logger.LogInformation("Generated metrics");
                return s.ToString();

            }
            catch (Exception ex)
            {
                _logger.LogError(string.Empty, ex);
            }

            return string.Empty;
        }

        #endregion
    }
}
