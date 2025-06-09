using AspNetCore.Live.Api.HealthChecks.Server;
using AspNetCore.Live.Api.HealthChecks.Server.Hubs;
using MongoDB.Driver;

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
            services.AddCors();

            services.AddSignalR();

            //Load the Clients dynamically
            services.AddScoped<IClientsService, ClientsService>();
            services.AddLiveHealthChecksServer(options =>
            {
                //Optional - Save Health Check info with Report to database.
                //Set UseDatabase flag to true.    
                options.UseDatabase = false;

                //If you want to use a custom database
                //Provide your own implementation of IServerRepository.
                //options.UseCustomDatabase = true;

                //Default database - MongoDB
                //Provide the MongoDB connection string.
                options.DatabaseConnectionString = "mongodb://localhost:27017/ServerDb";
                //Optional - Configure MongoClient.
                //options.Configure = sp => new MongoClient(options.DatabaseConnectionString);                
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithOrigins(new string[] { "https://localhost:7151" })
                .AllowCredentials());     // allow credentials

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<LiveHealthChecksHub>("/livehealthcheckshub");
            });
        }
    }
}
