using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Core;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Wbtb main loading/start class for web server. Registers types, calls wbtb core, starts daemons etc. 
    /// </summary>
    public class WbtbStart : BackgroundService
    {
        /// <summary>
        /// Called when HTTP endpoints have been registered internally by runtime, and app is ready to respond to incoming requests. 
        /// At this point we are still returing a friendly "server is busy" rersponse.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="lifetime"></param>
        public WbtbStart(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime)
        {
            ConfigurationBasic basicConfiguration = new ConfigurationBasic();

            // register types defined in web project - common types are registered in Core.cs
            SimpleDI di = new SimpleDI();

            di.RegisterFactory<ILogger, LogProvider>();
            Logger logger = new Logger();
            ILogger fileLogger = di.Resolve<ILogger>();
            logger.StatusVerbosityThreshold = basicConfiguration.StatusVerbosityThreshold;
            logger.DebugVerbosityThreshold = basicConfiguration.DebugVerbosityThreshold;
            logger.DebugSourceAllow = basicConfiguration.DebugSourceAllow;
            logger.DebugSourceBlock = basicConfiguration.DebugSourceBlock;

            // log debug out to inf for now because can't figure out where to safetly hardcode serilog
            // to give us debug output while also not have asp.net spam debug crap
            logger.OnDebug = (string message) => fileLogger.LogInformation(message);

            logger.OnError = (string message, object arg, string source) => fileLogger.LogError(message, arg, source);
            logger.OnStatus = (string message) => fileLogger.LogInformation(message);
            logger.OnWarn = (string message, object arg) => fileLogger.LogWarning(message);
            di.RegisterSingleton<Logger>(logger);

            logger.Status("=====================================================================");
            logger.Status("Wbtb load started");

            logger.Debug(this, "Log configured", 3);

            lifetime.ApplicationStarted.Register(() =>{
                try
                {
                    di.Register<MutationHelper, MutationHelper>();
                    di.Register<FailingAlertKey, FailingAlertKey>();
                    di.Register<IDaemonTaskController, DaemonTaskController>();
                    di.Register<IWebDaemon, BuildAlertQueueDaemon>(null, true);
                    di.Register<IWebDaemon, BuildStartDaemon>(null, true);
                    di.Register<IWebDaemon, BuildPostProcessDaemon>(null, true);
                    di.Register<IWebDaemon, BuildEndDaemon>(null, true);
                    di.Register<IWebDaemon, RevisionFromLogDaemon>(null, true);
                    di.Register<IWebDaemon, RevisionFromBuildServerDaemon>(null, true);
                    di.Register<IWebDaemon, LogImportDaemon>(null, true);
                    di.Register<IWebDaemon, BuildAlertDaemon>(null, true);
                    di.Register<IWebDaemon, IncidentAssignDaemon>(null, true);
                    di.Register<IWebDaemon, LogParseDaemon>(null, true);
                    di.Register<IWebDaemon, UserLinkDaemon>(null, true);
                    di.Register<IWebDaemon, RevisionLinkDaemon>(null, true);
                    di.Register<IWebDaemon, BuildBrokenRemindDaemon>(null, true);
                    di.RegisterSingleton(typeof(DaemonTaskProcesses), new DaemonTaskProcesses());
                    di.RegisterFactory<IHubContext, HubFactory>();
                    di.Register<MetricsHelper, MetricsHelper>();
                    di.Register<BuildEventHandlerHelper, BuildEventHandlerHelper>();
                    di.Register<Wbtb.Core.Core, Wbtb.Core.Core>();

                    logger.Debug(this, "Web.Core types registered", 3);

                    // Wbtb's core has common startup logic for web/CLI apps, such as setting up and validating config
                    Wbtb.Core.Core core = di.Resolve<Wbtb.Core.Core>();
                    core.Start();

                    // config should now be available
                    Configuration config = di.Resolve<Configuration>();

                    string disableDaemonsLook = Environment.GetEnvironmentVariable("WBTB_ENABLE_DAEMONS");
                    bool disableDaemons = disableDaemonsLook == "0" || disableDaemonsLook == "false" || config.EnabledDaemons == false;
                    string disableSocketsLook = Environment.GetEnvironmentVariable("WBTB_ENABLE_SOCKETS");
                    bool disableSockets = disableSocketsLook == "0" || disableSocketsLook == "false" || config.EnabledSockets == false;

                    // setup background processes / daemons etc for web
                    using (IServiceScope scope = serviceProvider.CreateScope())
                    {
                        if (disableDaemons)
                        {
                            logger.Status("DAEMONS DISABLED");
                        }
                        else
                        {
                            // start daemons by find all types that implement IWebDaemon
                            IEnumerable<IWebDaemon> webDaemons = di.ResolveAll<IWebDaemon>();
                            foreach (IWebDaemon daemon in webDaemons) 
                                daemon.Start(config.DaemonInterval * 1000);

                            logger.Status("Daemons started", 3);
                        }

                        if (disableSockets)
                        {
                            logger.Status("SOCKETS DISABLED");
                        }
                        else
                        {
                            // Start SignalR hub, this pipes all console Write and WriteLines to connected browsers
                            // Note : this is dev stuff, it should be moved somewhere better. 
                            IHubContext<ConsoleHub> hub = scope.ServiceProvider.GetService<IHubContext<ConsoleHub>>();
                            using (ConsoleWriter consoleWriter = new ConsoleWriter())
                            {
                                consoleWriter.WriteEvent += (object sender, ConsoleWriterEventArgs e) => {
                                    hub.Clients.All.SendAsync("ReceiveMessage", "some user", e.Value);
                                };
                                logger.Status("sockets started", 3);

                                Console.SetOut(consoleWriter);
                            }
                        }
                    }

                    // Everything is started. App is now ready to accept incoming requests. If false, HTTP endpoints
                    // will return a "server busy warming up" page.
                    AppState.Ready = true;
                    logger.Status("Wbtb load completed");
                    logger.Status("=====================================================================");

                }
                catch (ConfigurationException ex)
                {
                    AppState.ConfigErrors = true;
                    logger.Error(this, $"WBTB configuration error", ex);

                    // force exit app if config failed
                    if (basicConfiguration.ExitOnConfigError) 
                    {
                        logger.Status("WBTB start interrupted by configuration errors. App exiting.");
                        System.Diagnostics.Process
                            .GetCurrentProcess()
                            .Kill();
                    }
                }
            }); 
        }

        /// <summary>
        /// Reguired by BackgroundService base class.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.FromResult<object>(null);
        }
    }
}


