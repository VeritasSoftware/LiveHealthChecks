namespace Sample.Api
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
            services.AddRouting();
            services.AddAuthorization();

            // Add services to the container.
            services.AddHealthChecks()
                    .AddLiveHealthChecksClient(settings =>
                    {
                        //You can set the health check interval
                        //by a Cron Expression. 
                        settings.HealthCheckIntervalCronExpression = "* * * * *";
                        //Or in minutes
                        //settings.HealthCheckIntervalInMinutes = 1;
                        //Providing ClientId is optional. Good for tracking in the logs.
                        settings.ClientId = "SampleApi";
                                    settings.ReceiveMethod = "SampleApiHealth";
                                    settings.HealthCheckServerHubUrl = "https://localhost:5001/livehealthcheckshub";
                                    settings.SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692";
                                    settings.PublishOnlyWhenNotHealthy = false;
                        //Optional - transform your health report to as you want it published.
                        //settings.TransformHealthReport = healthReport => new
                        //{
                        //    status = healthReport.Status.ToString(),
                        //    results = healthReport.Entries.Select(e => new
                        //    {
                        //        key = e.Key,
                        //        value = e.Value.Status.ToString()
                        //    })
                        //};
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
        }
    }
}
