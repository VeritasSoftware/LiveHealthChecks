// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Hello, Monitoring app!");
Console.WriteLine(Environment.NewLine);

Thread.Sleep(1000 * 15);

var connection = new HubConnectionBuilder()
                        .WithUrl("https://localhost:5001/livehealthcheckshub", o =>
                        {
                            o.Headers.Add("LiveHealthChecks-ReceiveMethod", "SampleApiHealth");
                            o.Headers.Add("LiveHealthChecks-SecretKey", "43bf0968-17e0-4d22-816a-6eaadd766692");
                        })
                        .WithAutomaticReconnect()
                        .Build();

connection.On("SampleApiHealth", new Type[] {typeof(object), typeof(object)},
    (arg1, arg2) =>
    {
        Console.WriteLine(arg1[0]);
        return Task.CompletedTask;
    }, new object());

await connection.StartAsync();

Console.ReadLine();

