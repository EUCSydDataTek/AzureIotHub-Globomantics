using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Common;

// Hentes fra User Secrets. Findes som primary connectionstring for den enkelte device
IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

Console.WriteLine("Initializing Band Agent...");

DeviceClient device = DeviceClient.CreateFromConnectionString(configuration["DeviceConnectionString"], TransportType.Amqp);

await device.OpenAsync();

Console.WriteLine("Device is connected!");

await UpdateTwin(device);

Console.WriteLine("Press a key to perform an action:");
Console.WriteLine("q: quits");
Console.WriteLine("h: send happy feedback");
Console.WriteLine("u: send unhappy feedback");
Console.WriteLine("e: request emergency help");

StatusType status = StatusType.NotSpecified;
while (status != StatusType.Quit)
{
    Console.Write("Action? ");
    char input = Console.ReadKey().KeyChar;
    Console.WriteLine();
    
    int latitude = Random.Shared.Next(0, 100);
    int longitude = Random.Shared.Next(0, 100);

    status = char.ToLower(input) switch
    {
        'q' => StatusType.Quit,
        'h' => StatusType.Happy,
        'u' => StatusType.Unhappy,
        'e' => StatusType.Emergency,
        _ => StatusType.NotSpecified
    };

    Telemetry telemetry = new()
    {
        Latitude = latitude,
        Longitude = longitude,
        Status = status
    };
 
    string payload = JsonSerializer.Serialize(telemetry);
    Message message = new(Encoding.ASCII.GetBytes(payload));

    await device.SendEventAsync(message);

    Console.WriteLine("Message sent!");
}

Console.WriteLine("Disconnecting...");

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
return;

static async Task UpdateTwin(DeviceClient device)
{
    TwinCollection twinProperties = new()
    {
        ["connectionType"] = "wifi",
        ["connectionStrength"] = "high"
    };

    await device.UpdateReportedPropertiesAsync(twinProperties);
}