using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Wbtb.Core.Common;
using Wbtb.Core.Web.Controllers;

namespace Wbtb.Core.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages()
                .AddRazorRuntimeCompilation(); // allows us to to reload view changes without recompile

            services.AddSignalR();
            
            services.AddTransient<IMessageQueue, MessageQueue>();
            services.AddHostedService<ServerStartService>();
            // force built-in DI to use our DI's controller factory, this is the only known way to bypass M$' DI for controllers
            services.AddSingleton<IControllerFactory, ControllerFactory>();
            services.AddMemoryCache();

            SimpleDI di = new SimpleDI();

            di.Register<LogHelper, LogHelper>();   
            di.Register<PluginDirectSender, PluginDirectSender>();
            di.Register<PluginCoreSender, PluginCoreSender>(); 
            di.Register<PersistPathHelper, PersistPathHelper>();
            di.Register<MessageQueueHtppClient, MessageQueueHtppClient>();
            di.Register<ConfigBasic, ConfigBasic>();
            di.Register<ConfigBootstrapper, ConfigBootstrapper>();
            di.Register<GitHelper, GitHelper>();
            di.Register<BuildLogParseResultHelper, BuildLogParseResultHelper>();
            di.Register<ConfigurationBuilder, ConfigurationBuilder>();
            di.Register<PluginProvider, PluginProvider>();
            di.Register<PluginManager, PluginManager>();
            di.Register<BuildLevelPluginHelper, BuildLevelPluginHelper>();
            di.Register<HomeController, HomeController>();
            di.Register<BuildController, BuildController>();
            di.Register<InvokeController, InvokeController>();
            di.Register<JobController, JobController>();
            di.Register<IDaemonProcessRunner, DaemonProcessRunner>();
            di.Register<IWebDaemon, BuildImportDaemon>(true);
            di.Register<IWebDaemon, UserBuildInvolvementLinkDaemon>(true);
            di.Register<IWebDaemon, RevisionResolveDaemon>(true);
            di.Register<IWebDaemon, LogParseDaemon>(true);
            di.Register<IWebDaemon, BuildRevisionFromLogDaemon>(true);
            di.Register<IWebDaemon, IncidentAssignDaemon>(true);
            di.Register<FileSystemHelper, FileSystemHelper>();
            di.Register<CustomEnvironmentArgs, CustomEnvironmentArgs>();
            di.RegisterFactory<ILogger, LogProvider>();
            di.RegisterFactory<IHubContext, HubFactory>();
            di.RegisterFactory<IPluginSender, PluginSenderFactory>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider provider, IWebHostEnvironment env)
        {
            IHubContext<ConsoleHub> consoleHubContext = provider.GetRequiredService<IHubContext<ConsoleHub>>();
            HubFactoryGlobals.Add<IHubContext<ConsoleHub>>(consoleHubContext);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
                endpoints.MapHub<ConsoleHub>("/console-signalr");
            });
        }

        private void ConsoleWriter_WriteEvent(object sender, ConsoleWriterEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

