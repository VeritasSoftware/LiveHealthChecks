Console.WriteLine("Hello, Api!");
Console.WriteLine(Environment.NewLine);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHealthChecks()
                .AddLiveHealthChecksClient(settings =>
                {
                    settings.HealthCheckIntervalInMinutes = 30;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
