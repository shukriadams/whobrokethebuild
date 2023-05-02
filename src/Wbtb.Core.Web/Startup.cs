using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Wbtb.Core.Common;

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

            services.AddMemoryCache();

            LowEffortDI di = new LowEffortDI();
            di.Register<Configuration.ConfigurationBuilder, Configuration.ConfigurationBuilder>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

