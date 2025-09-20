using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using System;
using System.Collections.Generic;
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

            Serilog.Core.Logger fileLogger = new LoggerConfiguration()
                .MinimumLevel.Override("Wbtb", LogEventLevel.Debug)
                .WriteTo
                    .File(conf.LogPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();



            ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog(fileLogger);
            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger(service);

            return logger;
        }
    }
    
}
