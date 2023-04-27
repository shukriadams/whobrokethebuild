using Microsoft.Extensions.Logging;
using System.IO;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public class LogHelper
    {
        /// <summary>
        /// Creates an instance of a logger without IOC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILogger GetILogger<T>()
        {
            // todo : log level from settings
            using ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Warning)
                .AddConsole());

            return loggerFactory.CreateLogger<T>();
        }

        public static void WriteBuildProcessorLog(string buildId, string buildProcessorId, string text)
        { 
            string path = Path.Join(ConfigKeeper.Instance.DataDirectory, "BuildProcessorLogs", buildId);
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Join(path, $"{buildProcessorId}.txt"), text);
        }

        public static string GetBuildProcessorLog(string buildId, string buildProcessorId)
        {
            string path = Path.Join(ConfigKeeper.Instance.DataDirectory, "BuildProcessorLogs", buildId, $"{buildProcessorId}.txt");
            if (!File.Exists(path))
                return string.Empty;

            return File.ReadAllText(path);
        }
    }
}
