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
            bool exitOnConfigError = false;

            lifetime.ApplicationStarted.Register(() =>{
                try
                {
                    // register types defined in web project - common types are registered in Core.cs
                    SimpleDI di = new SimpleDI();
                    di.RegisterFactory<ILogger, LogProvider>();
                    di.Register<Logger, Logger>();
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

                    // Default behaviour is to not exit on config error, but to rather park application in a 
                    // mode where endpoints all return "config broken please fix". This can be overridden. 
                    // Do this before calling core, as it can throw expected ConfigurationExceptions.
                    // Note that we haven't yet loaded .env file, so this is the one Env var that must be
                    // set higher up, ie, on the parent process/system level, in VStudio before launching app, etc.
                    string exitOnConfigErrorLook = Environment.GetEnvironmentVariable("WBTB_EXIT_ON_CONFIG_ERROR");
                    if (exitOnConfigErrorLook == "1" || exitOnConfigErrorLook == "true") { 
                        exitOnConfigError = true;
                        ConsoleHelper.WriteLine("exit on config error is enabled");
                    }

                    // Wbtb's core has common startup logic for web/CLI apps, such as setting up and validating config
                    Wbtb.Core.Core core = new Wbtb.Core.Core();
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
                            ConsoleHelper.WriteLine("DAEMONS DISABLED");
                        }
                        else
                        {
                            // start daemons by find all types that implement IWebDaemon
                            IEnumerable<IWebDaemon> webDaemons = di.ResolveAll<IWebDaemon>();
                            foreach (IWebDaemon daemon in webDaemons)
                                daemon.Start(config.DaemonInterval * 1000);
                        }

                        if (disableSockets)
                        {
                            ConsoleHelper.WriteLine("SOCKETS DISABLED");
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

                                Console.SetOut(consoleWriter);
                            }
                        }
                    }

                    // Everything is started. App is now ready to accept incoming requests. If false, HTTP endpoints
                    // will return a "server busy warming up" page.
                    AppState.Ready = true;
                }
                catch (ConfigurationException ex)
                {
                    AppState.ConfigErrors = true;

                    ConsoleHelper.WriteLine($"WBTB configuration error : {ex.Message}");

                    if (exitOnConfigError) 
                    {
                        ConsoleHelper.WriteLine("WBTB failed to start - configuration errors were detected :");
                        // force exit app if config failed
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


