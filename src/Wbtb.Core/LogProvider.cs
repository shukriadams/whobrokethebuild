using Microsoft.Extensions.Logging;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public class LogProvider : ISimpleDIFactory
    {
        public object Resolve<T>()
        {
            // todo : get log level from config
            using ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Warning)
                .AddConsole());

            return loggerFactory.CreateLogger<T>();
        }
    }
}
