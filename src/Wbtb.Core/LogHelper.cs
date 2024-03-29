﻿using System.IO;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public class LogHelper
    {
        private readonly Configuration _config;

        public LogHelper(Configuration config) 
        {
            _config = config;
        }

        public string GetBuildProcessorLog(string buildId, string buildProcessorId)
        {
            string path = Path.Join(_config.DataRootPath, "BuildProcessorLogs", buildId, $"{buildProcessorId}.txt");
            if (!File.Exists(path))
                return string.Empty;

            return File.ReadAllText(path);
        }
    }
}
