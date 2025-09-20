using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// First type run by the server. Entry point, main, whatever. Hands over to AspStart.
    /// </summary>
    public class ProgramStart
    {
        public static object LockInstance = new object();

        public static void Main(string[] args)
        {
            // pick up env vars from local .env file as early as possible in app load cycle, the contents of this file can be used to set basic features in application
            // and needs to be loaded before anything else
            CustomEnvironmentArgs customEnvironmentArgs = new CustomEnvironmentArgs();
            customEnvironmentArgs.Apply(true);

            // validate important env vars as early as possible in app lifecylce
            ConfigurationBasic.ValidateAndOverrideDefaults();

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
                        throw new ConfigurationException($"The provided log path {configurationBasic.DotNetLogPath} is invalid - path must be a file, but there is currently a directory at this location.");

                    Directory.CreateDirectory(Path.GetDirectoryName(configurationBasic.DotNetLogPath));

                    // set default log level to warning, else underyling asp frameworks spams log with garbage
                    builder.AddFile(configurationBasic.DotNetLogPath, LogLevel.Warning); 
                })

                // hand over too AspStart to continue loading
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<AspStart>();
                });
    }
}
