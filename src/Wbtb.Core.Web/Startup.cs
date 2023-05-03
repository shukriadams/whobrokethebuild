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
using Wbtb.Core.Common.Plugins;
using Wbtb.Core.Web.Controllers;
using Wbtb.Core.Web.Core;

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
            services.AddTransient(typeof(IDaemonProcessRunner), typeof(DaemonProcessRunner));
            services.AddTransient(typeof(IWebDaemon), typeof(BuildImportDaemon));
            services.AddTransient(typeof(IWebDaemon), typeof(UserBuildInvolvementLinkDaemon));
            services.AddTransient(typeof(IWebDaemon), typeof(RevisionResolveDaemon));
            services.AddTransient(typeof(IWebDaemon), typeof(LogParseDaemon));
            services.AddTransient(typeof(IWebDaemon), typeof(BuildRevisionFromLogDaemon));
            services.AddTransient(typeof(IWebDaemon), typeof(IncidentAssignDaemon));
            // force built-in DI to use our DI's controller factory, this is the only known way to bypass M$' DI for controllers
            services.AddSingleton<IControllerFactory, ControllerFactory>();

            services.AddMemoryCache();

            SimpleDI di = new SimpleDI();

            di.Register<Configuration.ConfigurationBuilder, Configuration.ConfigurationBuilder>();
            di.Register<PluginProvider, PluginProvider>();
            di.Register<PluginManager, PluginManager>();
            di.Register<BuildLevelPluginHelper, BuildLevelPluginHelper>();
            di.Register<HomeController, HomeController>();
            di.Register<BuildController, BuildController>();
            di.Register<InvokeController, InvokeController>();
            di.Register<JobController, JobController>();
            di.RegisterFactory<ILogger, LogProvider>();
            di.RegisterFactory<IHubContext, HubFactory>();
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

