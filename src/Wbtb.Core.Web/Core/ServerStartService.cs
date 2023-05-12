using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class ServerStartService : BackgroundService
    {
        public ServerStartService(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime)
        {

            lifetime.ApplicationStarted.Register(() =>{
                try
                {

                    bool hashChanged = Wbtb.Core.Core.EnsureConfig();
                    // if config changed, cleanly stop app, this will give us a chance to restart with new config. 
                    // in a containerized environemnt this should happen automatically
                    if (hashChanged)
                        lifetime.StopApplication();

                    Wbtb.Core.Core.StartServer();

                    SimpleDI di = new SimpleDI();
                    Config config = di.Resolve<Config>();

                    foreach (PluginConfig plugin in config.Plugins.Where(p=> p.Manifest.RuntimeParsed == Runtimes.dotnet)) 
                    {
                        Type interfaceType = TypeHelper.GetCommonType(plugin.Manifest.Interface);
                        
                        if (!plugin.Proxy)
                            TypeHelper.GetAssembly(plugin.Manifest.Assembly); // force load assembly

                        Type implementation = plugin.Proxy ? TypeHelper.GetRequiredProxyType(interfaceType) : TypeHelper.ResolveType(plugin.Manifest.Concrete);
                        if (implementation == null)
                            throw new ConfigurationException($"Could not resolve plugin type {plugin.Manifest.Concrete}");

                        PluginBehaviourAttribute pluginBehaviour = TypeHelper.GetAttribute<PluginBehaviourAttribute>(interfaceType);

                        di.Register(interfaceType, implementation, key : plugin.Key, allowMultiple : pluginBehaviour.AllowMultiple);
                    }

                    Wbtb.Core.Core.LoadPlugins();

                    string disableDaemonsLook = Environment.GetEnvironmentVariable("WBTB_ENABLE_DAEMONS");
                    bool disableDaemons = disableDaemonsLook == "0" || disableDaemonsLook == "false" || config.EnabledDaemons == false;

                    // these shoule be moved to "startserver" too
                    if (disableDaemons)
                    {
                        Console.WriteLine("DAEMONS DISABLED");
                    }
                    else 
                    {
                        using (IServiceScope scope = serviceProvider.CreateScope())
                        {
                            // start daemons by find all types that implement IWebdaemon, start all
                            IEnumerable<IWebDaemon> webDaemons = di.ResolveAll<IWebDaemon>(); // scope.ServiceProvider.GetServices<IWebDaemon>();
                            foreach (IWebDaemon daemon in webDaemons)
                                daemon.Start(config.DaemonInterval * 1000);

                            // signlr, pipe all console write and writelines to signlr hub
                            // dev stuff, this should be moved somewhere better. 
                            IHubContext<ConsoleHub> hub = scope.ServiceProvider.GetService<IHubContext<ConsoleHub>>();
                            using (var consoleWriter = new ConsoleWriter())
                            {
                                consoleWriter.WriteEvent += (object sender, ConsoleWriterEventArgs e) => {
                                    hub.Clients.All.SendAsync("ReceiveMessage", "some user", e.Value);
                                };
                                Console.SetOut(consoleWriter);
                            }
                        }
                    }
                }
                catch (ConfigurationException ex)
                {
                    Console.WriteLine("WBTB failed to start - configuration errors were detected :");
                    Console.WriteLine(ex.Message);

                    // force exit app if config failed
                    System.Diagnostics.Process
                        .GetCurrentProcess()
                        .Kill();
                }
            }); 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.FromResult<object>(null);
        }
    }
}
