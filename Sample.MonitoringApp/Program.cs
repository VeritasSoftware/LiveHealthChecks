// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Hello, Monitoring app!");
Console.WriteLine(Environment.NewLine);

Thread.Sleep(1000 * 15);

var connection = new HubConnectionBuilder()
                        .WithUrl("https://localhost:5001/livehealthcheckshub")
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

