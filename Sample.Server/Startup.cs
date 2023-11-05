using AspNetCore.Live.Api.HealthChecks.Server;
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
            services.AddCors(options =>
            {
                options.AddPolicy("BlazorWasm", builder => builder.WithOrigins("https://localhost:7151").AllowAnyHeader().AllowAnyMethod().AllowCredentials());
            });

            services.AddSignalR();

            //Load the Clients dynamically
            services.AddScoped<IClientsService, ClientsService>();
            services.AddLiveHealthChecksServer(options =>
            {
                //Optional - Save Health Check info with Report to MongoDB database.
                //Set flag to true.
                //Provide the MongoDB connection string.
                options.UseDatabase = true;
                options.DatabaseConnectionString = Configuration.GetConnectionString("MongoDb");
            });
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
