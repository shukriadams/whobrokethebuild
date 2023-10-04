using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{

    public class LogProvider : ISimpleDIFactory
    {
        public object Resolve<T>()
        {
            return this.Resolve(typeof(T));
        }

        public object Resolve(Type service)
        {
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddLogging((loggingBuilder) => loggingBuilder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddConsole()
                    )
                .BuildServiceProvider();

            ILogger logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();

            return logger;
        }
    }

}
