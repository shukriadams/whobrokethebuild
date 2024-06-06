using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class Program
    {
        public static object LockInstance = new object();

        public static void Main(string[] args)
        {
            // pick up env vars from local .env file as early as possible in app load cycle, the contents of this file can be used to set basic features in application
            // and needs to be loaded before anything else
            CustomEnvironmentArgs customEnvironmentArgs = new CustomEnvironmentArgs();
            customEnvironmentArgs.Apply(true);
        
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Override to set logging, the continue loading via Startup class.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                
                .ConfigureLogging((hostingContext, builder) => {

                    ConfigurationBasic configurationBasic = new ConfigurationBasic();

                    if (Directory.Exists(configurationBasic.DotNetLogPath))
                        throw new ConfigurationException($"The provided logpath {configurationBasic.DotNetLogPath} is invalid - path must be a file, but there is currently a directory at this location.");

                    Directory.CreateDirectory(Path.GetDirectoryName(configurationBasic.DotNetLogPath));
                    builder.AddFile(configurationBasic.DotNetLogPath, LogLevel.Warning);

                })

                // continue laoding
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
