using Microsoft.Extensions.Logging;
using System;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public class LogProvider : ISimpleDIFactory
    {
        public object Resolve<T>()
        {
            return this.Resolve(typeof(T));
        }

        public object Resolve(Type service)
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Warning)
                .AddConsole());

            return loggerFactory.CreateLogger(service);

        }
    }
}
