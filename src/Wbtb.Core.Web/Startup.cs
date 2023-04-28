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
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            services.AddTransient(typeof(IDataLayerPlugin), typeof(Postgres));
            services.AddTransient(typeof(IAuthenticationPlugin), typeof(ActiveDirectory));
            services.AddTransient(typeof(IAuthenticationPlugin), typeof(ActiveDirectorySandbox));
            services.AddTransient(typeof(IAuthenticationPlugin), typeof(Internal));
            services.AddTransient(typeof(IBuildServerPlugin), typeof(Extensions.BuildServer.Jenkins.Jenkins));
            services.AddTransient(typeof(IBuildServerPlugin), typeof(Extensions.BuildServer.JenkinsSandbox.JenkinsSandbox));
            services.AddTransient(typeof(ISourceServerPlugin), typeof(Perforce));
            services.AddTransient(typeof(ISourceServerPlugin), typeof(Extensions.SourceServer.PerforceSandbox.PerforceSandbox));
            services.AddTransient(typeof(IMessaging), typeof(Slack));
            services.AddTransient(typeof(ILogParser), typeof(Unreal4));
            services.AddTransient(typeof(ILogParser), typeof(Cpp));
            services.AddTransient(typeof(ILogParser), typeof(JenkinsSelfFailing));
            services.AddTransient(typeof(IWebDaemon), typeof(BuildImportDaemon));
            services.AddTransient(typeof(IWebDaemon), typeof(UserBuildInvolvementLinkDaemon));
            services.AddTransient(typeof(IWebDaemon), typeof(RevisionResolveDaemon));
            services.AddTransient(typeof(IWebDaemon), typeof(LogParseDaemon));
            services.AddTransient(typeof(IWebDaemon), typeof(BuildRevisionFromLogDaemon));
            services.AddTransient(typeof(IWebDaemon), typeof(IncidentAssignDaemon));
            services.AddTransient(typeof(IDaemonProcessRunner), typeof(DaemonProcessRunner));
            services.AddTransient(typeof(IPostProcessor), typeof(Extensions.PostProcessing.Test.Test));
            services.AddTransient(typeof(IPostProcessor), typeof(Extensions.PostProcessing.Test2.Test2));
            
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
            kernel.Bind(typeof(IDataLayerPlugin)).To(typeof(Postgres));
            kernel.Bind(typeof(IAuthenticationPlugin)).To(typeof(ActiveDirectory));
            kernel.Bind(typeof(IAuthenticationPlugin)).To(typeof(ActiveDirectorySandbox));
            kernel.Bind(typeof(IAuthenticationPlugin)).To(typeof(Internal));
            //kernel.Bind<IBuildServerPlugin>().To<Extensions.BuildServer.Jenkins.Jenkins>();
            kernel.Bind(typeof(IBuildServerPlugin)).To(typeof(Extensions.BuildServer.JenkinsSandbox.JenkinsSandbox));
            kernel.Bind(typeof(ISourceServerPlugin)).To(typeof(Perforce));
            kernel.Bind(typeof(ISourceServerPlugin)).To(typeof(Extensions.SourceServer.PerforceSandbox.PerforceSandbox));
            kernel.Bind(typeof(IMessaging)).To(typeof(Slack));
            kernel.Bind(typeof(ILogParser)).To(typeof(Unreal4));
            kernel.Bind(typeof(ILogParser)).To(typeof(Cpp));
            kernel.Bind(typeof(ILogParser)).To(typeof(JenkinsSelfFailing));

            kernel.Bind(typeof(IPostProcessor)).To(typeof(Extensions.PostProcessing.Test.Test));
            kernel.Bind(typeof(IPostProcessor)).To(typeof(Extensions.PostProcessing.Test2.Test2));

            PluginProvider.Factory = new NinjectWrapper(kernel);
        }

        private void ConsoleWriter_WriteEvent(object sender, ConsoleWriterEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

