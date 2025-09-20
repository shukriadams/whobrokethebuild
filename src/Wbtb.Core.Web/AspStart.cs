using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Microsoft-standard start handler for Asp applications. This is the second type run on app start. It is called by
    /// Program.cs, and in turn hands over to WbtbStart.
    /// </summary>
    public class AspStart
    {
        public IConfiguration Configuration { get; }

        public AspStart(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages()
                .AddRazorRuntimeCompilation(); // allows us to to reload view changes without recompile

            // note : we don't use dotnet's DI framework. DI is handled by internal SimpleDI
            services.AddSignalR();
            services.AddMemoryCache();
            // most WBTB app load logic is in this service, which is called once the underlying ASP application is loaded.
            services.AddHostedService<WbtbStart>();

            services.AddScoped<ViewStatus>();
        }

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
                app.UseExceptionHandler("/error/500");
            }

            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
                {
                    context.Request.Path = "/error/404";
                    await next();
                }
            });

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

