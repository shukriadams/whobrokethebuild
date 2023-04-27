using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Wbtb.Core.Common;
using Wbtb.Extensions.Data.Postgres;
using Ninject;
using Wbtb.Core.Common.Plugins;
using Wbtb.Extensions.Auth.ActiveDirectory;
using Wbtb.Extensions.SourceServer.Perforce;
using Wbtb.Extensions.Messaging.Slack;
using Wbtb.Extensions.LogParsing.Unreal;
using Wbtb.Extensions.LogParsing.JenkinsSelfFailing;
using Wbtb.Extensions.LogParsing.Cpp;
using Wbtb.Extensions.Auth.Internal;
using Wbtb.Extensions.Auth.ActiveDirectorySandbox;

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

            // DEV ONLY! - these are needed by controllers only
            services.AddTransient<IDataLayerPlugin, Postgres>();
            services.AddTransient<IAuthenticationPlugin, ActiveDirectory>();
            services.AddTransient<IAuthenticationPlugin, ActiveDirectorySandbox>();
            services.AddTransient<IAuthenticationPlugin, Internal>();
            services.AddTransient<IBuildServerPlugin, Extensions.BuildServer.Jenkins.Jenkins>();
            services.AddTransient<IBuildServerPlugin, Extensions.BuildServer.JenkinsSandbox.JenkinsSandbox>();
            services.AddTransient<ISourceServerPlugin, Perforce>();
            services.AddTransient<ISourceServerPlugin, Extensions.SourceServer.PerforceSandbox.PerforceSandbox>();
            services.AddTransient<IMessaging, Slack>();
            services.AddTransient<ILogParser, Unreal4>();
            services.AddTransient<ILogParser, Cpp>();
            services.AddTransient<ILogParser, JenkinsSelfFailing>();
            services.AddTransient<IWebDaemon, BuildImportDaemon>();
            services.AddTransient<IWebDaemon, UserBuildInvolvementLinkDaemon>();
            services.AddTransient<IWebDaemon, RevisionResolveDaemon>();
            services.AddTransient<IWebDaemon, LogParseDaemon>();
            services.AddTransient<IWebDaemon, BuildRevisionFromLogDaemon>();
            services.AddTransient<IWebDaemon, IncidentAssignDaemon>();
            services.AddTransient<IDaemonProcessRunner, DaemonProcessRunner>();
            services.AddTransient<IPostProcessor, Extensions.PostProcessing.Test.Test>();
            services.AddTransient<IPostProcessor, Extensions.PostProcessing.Test2.Test2>();
            
            services.AddHostedService<ServerStartService>();

            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, IHostApplicationLifetime lifetime, IWebHostEnvironment env, ILoggerFactory loggerFactory)
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

            // bind types - dev only! These are needed by all general plugin activity
            StandardKernel kernel = new StandardKernel();
            kernel.Bind<IDataLayerPlugin>().To<Postgres>();
            kernel.Bind<IAuthenticationPlugin>().To<ActiveDirectory>();
            kernel.Bind<IAuthenticationPlugin>().To<ActiveDirectorySandbox>();
            kernel.Bind<IAuthenticationPlugin>().To<Internal>();
            //kernel.Bind<IBuildServerPlugin>().To<Extensions.BuildServer.Jenkins.Jenkins>();
            kernel.Bind<IBuildServerPlugin>().To<Extensions.BuildServer.JenkinsSandbox.JenkinsSandbox>();
            kernel.Bind<ISourceServerPlugin>().To<Perforce>();
            kernel.Bind<ISourceServerPlugin>().To<Extensions.SourceServer.PerforceSandbox.PerforceSandbox>();
            kernel.Bind<IMessaging>().To<Slack>();
            kernel.Bind<ILogParser>().To<Unreal4>();
            kernel.Bind<ILogParser>().To<Cpp>();
            kernel.Bind<ILogParser>().To<JenkinsSelfFailing>();

            kernel.Bind<IPostProcessor>().To<Extensions.PostProcessing.Test.Test>();
            kernel.Bind<IPostProcessor>().To<Extensions.PostProcessing.Test2.Test2>();

            PluginProvider.Factory = new NinjectWrapper(kernel);
        }

        private void ConsoleWriter_WriteEvent(object sender, ConsoleWriterEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

