using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
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
            ConfigurationBasic conf = new ConfigurationBasic();

            Logger fileLogger = new LoggerConfiguration()
                .MinimumLevel.Override("Wbtb", Serilog.Events.LogEventLevel.Verbose)
                .WriteTo.File(conf.LogPath, rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog(fileLogger);
            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger(service);

            return logger;
        }
    }
    
}
