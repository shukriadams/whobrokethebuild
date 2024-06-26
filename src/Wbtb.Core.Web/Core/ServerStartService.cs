﻿using Microsoft.AspNetCore.SignalR;
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
    public class ServerStartService : BackgroundService
    {
        /// <summary>
        /// Does the final part of Wbtb server start, after the underlying ASP application is loaded. We need to wait for this because WBTB relies on HTTP requests to do its thing.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="lifetime"></param>
        public ServerStartService(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime)
        {
            bool exitOnConfigError = true;

            lifetime.ApplicationStarted.Register(() =>{
                try
                {
                    SimpleDI di = new SimpleDI();

                    // register types defined in web project
                    di.RegisterFactory<ILogger, LogProvider>();
                    di.Register<MutationHelper, MutationHelper>();
                    di.Register<IDaemonProcessRunner, DaemonProcessRunner>();
                    di.Register<IWebDaemon, BuildStartDaemon>(null, true);
                    di.Register<IWebDaemon, BuildPostProcessDaemon>(null, true);
                    di.Register<IWebDaemon, BuildEndDaemon>(null, true);
                    di.Register<IWebDaemon, RevisionFromLogDaemon>(null, true);
                    di.Register<IWebDaemon, RevisionFromBuildServerDaemon>(null, true);
                    di.Register<IWebDaemon, LogImportDaemon>(null, true);
                    di.Register<IWebDaemon, BuildDeltaAlertDaemon>(null, true);
                    di.Register<IWebDaemon, IncidentAssignDaemon>(null, true);
                    di.Register<IWebDaemon, LogParseDaemon>(null, true);
                    di.Register<IWebDaemon, UserLinkDaemon>(null, true);
                    di.Register<IWebDaemon, RevisionLinkDaemon>(null, true);
                    di.Register<IWebDaemon, BuildBrokenRemindDaemon>(null, true);
                    di.RegisterSingleton(typeof(DaemonTaskProcesses), new DaemonTaskProcesses());
                    di.RegisterFactory<IHubContext, HubFactory>();
                    di.Register<BuildEventHandlerHelper, BuildEventHandlerHelper>();


                    string exitOnConfigErrorLook = Environment.GetEnvironmentVariable("WBTB_EXIT_ON_CONFIG_ERROR");

                    if (exitOnConfigErrorLook == "0" || exitOnConfigErrorLook == "false") { 
                        exitOnConfigError = false;
                        ConsoleHelper.WriteLine("exit on config error is disabled");
                    }
                    exitOnConfigError = false;

                    Wbtb.Core.Core core = new Wbtb.Core.Core();
                    core.Start();


                    Configuration config = di.Resolve<Configuration>();

                    string disableDaemonsLook = Environment.GetEnvironmentVariable("WBTB_ENABLE_DAEMONS");
                    bool disableDaemons = disableDaemonsLook == "0" || disableDaemonsLook == "false" || config.EnabledDaemons == false;
                    string disableSocketsLook = Environment.GetEnvironmentVariable("WBTB_ENABLE_SOCKETS");
                    bool disableSockets = disableSocketsLook == "0" || disableSocketsLook == "false" || config.EnabledSockets == false;

                    using (IServiceScope scope = serviceProvider.CreateScope())
                    {
                        if (disableDaemons)
                        {
                            ConsoleHelper.WriteLine("DAEMONS DISABLED");
                        }
                        else
                        {
                            // start daemons by find all types that implement IWebdaemon, start all
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
                            // signlr, pipe all console write and writelines to signlr hub
                            // dev stuff, this should be moved somewhere better. 
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

                    // app is ready to accept incoming requests
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.FromResult<object>(null);
        }
    }
}


