using Sample.Api;

Console.WriteLine("Hello, Api!");
Console.WriteLine(Environment.NewLine);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHealthChecks() //Required - add all your health checks
                .AddCheck<SampleHealthCheck>("Sample Health Check Api 1")
                //Add Live Health Checks Client
                .AddLiveHealthChecksClient(settings =>
                {
                    //You can set the health check interval
                    //by a Cron Expression. 
                    settings.HealthCheckIntervalCronExpression = "* * * * *";
                    //Or in minutes
                    //settings.HealthCheckIntervalInMinutes = 1;
                    settings.HealthCheckServerHubUrl = "https://localhost:5001/livehealthcheckshub";                    
                    settings.ReceiveMethod = "SampleApiHealth";                    
                    settings.SecretKey = "43bf0968-17e0-4d22-816a-6eaadd766692";
                    //Providing ClientId is optional. Good for tracking in the logs.
                    settings.ClientId = "Sample Api";
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


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{    
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();    
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.Run();
