// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Hello, Monitoring app!");
Console.WriteLine(Environment.NewLine);

Thread.Sleep(1000 * 15);

var connection = new HubConnectionBuilder()
                        .WithUrl("https://localhost:5001/livehealthcheckshub", o =>
                        {                            
                            o.Headers.Add("LiveHealthChecks-ReceiveMethod", "*");
                            o.Headers.Add("LiveHealthChecks-SecretKey", "f22f3fd2-687d-48a1-aa2f-f2c9181364eb");
                            //Optional - value can be anything you want. good for tracking in the logs.
                            o.Headers.Add("LiveHealthChecks-ClientId", "Monitoring App 1");
                        })
                        .WithAutomaticReconnect()
                        .Build();

connection.On<string>("SampleApiHealth", report =>
{
    Console.WriteLine(report);
});

////With a * ReceiveMethod header, you can add multiple ReceiveMethods, on the same connection.
//connection.On<string>("SampleApi2Health", report =>
//{
//    Console.WriteLine(report);
//});

await connection.StartAsync();

Console.ReadLine();

