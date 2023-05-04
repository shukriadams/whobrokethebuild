using Microsoft.Extensions.Logging;
using System.IO;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public class LogHelper
    {
        private readonly Config _config;

        public LogHelper(Config config) 
        {
            _config = config;
        }

        public string GetBuildProcessorLog(string buildId, string buildProcessorId)
        {
            string path = Path.Join(_config.DataDirectory, "BuildProcessorLogs", buildId, $"{buildProcessorId}.txt");
            if (!File.Exists(path))
                return string.Empty;

            return File.ReadAllText(path);
        }
    }
}
