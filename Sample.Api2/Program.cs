using Sample.Api2;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHealthChecks()
                .AddCheck<SampleHealthCheck>("Sample Health Check Api 2")
                .AddLiveHealthChecksClient(settings =>
                {
                    //You can set the health check interval
                    //by a Cron Expression. 
                    settings.HealthCheckIntervalCronExpression = "* * * * *";
                    //Or in minutes
                    //settings.HealthCheckIntervalInMinutes = 1;
                    //Providing ClientId is optional. Good for tracking in the logs.
                    settings.ClientId = "Sample Api 2";
                    settings.ReceiveMethod = "SampleApi2Health";
                    settings.HealthCheckServerHubUrl = "https://localhost:5001/livehealthcheckshub";
                    settings.SecretKey = "ae6f9a48-259b-4d03-9956-a2bf8838aaa4";
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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
