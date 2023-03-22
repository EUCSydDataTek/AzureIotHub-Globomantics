﻿using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Common;

// Hentes fra User Secrets. Findes som primary connectionstring for den enkelte device

var configuration = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

Console.WriteLine("Initializing Band Agent...");

var device = DeviceClient.CreateFromConnectionString(configuration["DeviceConnectionString"], TransportType.Amqp );

await device.OpenAsync();

Console.WriteLine("Device is connected!");

await UpdateTwin(device);

Console.WriteLine("Press a key to perform an action:");
Console.WriteLine("q: quits");
Console.WriteLine("h: send happy feedback");
Console.WriteLine("u: send unhappy feedback");
Console.WriteLine("e: request emergency help");

var random = new Random();
var quitRequested = false;

while (!quitRequested)
{
    Console.Write("Action? ");
    var input = Console.ReadKey().KeyChar;
    Console.WriteLine();

    var status = StatusType.NotSpecified;
    var latitude = random.Next(0, 100);
    var longitude = random.Next(0, 100);

    switch (char.ToLower(input))
    {
        case 'q':
            quitRequested = true;
            break;
        case 'h':
            status = StatusType.Happy;
            break;
        case 'u':
            status = StatusType.Unhappy;
            break;
        case 'e':
            status = StatusType.Emergency;
            break;
    }

    var telemetry = new Telemetry
    {
        Latitude = latitude,
        Longitude = longitude,
        Status = status
    };

 
    string payload = JsonSerializer.Serialize(telemetry, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    var message = new Message(Encoding.ASCII.GetBytes(payload));

    await device.SendEventAsync(message);

    Console.WriteLine("Message sent!");
}

Console.WriteLine("Disconnecting...");


await UpdateTwin(device);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

static async Task UpdateTwin(DeviceClient device)
{
    var twinProperties = new TwinCollection();
    twinProperties["connectionType"] = "wifi";
    twinProperties["connectionStrength"] = "full";

    await device.UpdateReportedPropertiesAsync(twinProperties);
}
