using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Wbtb.Core.Web
{
    public class Program
    {
        public static object LockInstance = new object();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, builder) => {

                    // set up logging
                    string logPath = Environment.GetEnvironmentVariable("WBTB_LOG_PATH");
                    if (string.IsNullOrEmpty(logPath))
                        logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "logs", "log.txt");

                    Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                    builder.AddFile(logPath);
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
