﻿using AspNetCore.Live.Api.HealthChecks.Server;
using AspNetCore.Live.Api.HealthChecks.Server.Hubs;

namespace Sample.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add serices to the container.
        public void ConfigureServices(IServiceCollection services)
        {            
            services.AddSignalR();

            //Load the Clients dynamically
            services.AddScoped<IClientsService, ClientsService>();
            services.AddLiveHealthChecksServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<LiveHealthChecksHub>("/livehealthcheckshub");
            });
        }
    }
}
