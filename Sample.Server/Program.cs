// See https://aka.ms/new-console-template for more information
using Sample.Server;

Console.WriteLine("Hello, Server!");
Console.WriteLine(Environment.NewLine);

var builder = WebApplication.CreateBuilder(args);
var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

startup.Configure(app, app.Environment);

app.Run();
