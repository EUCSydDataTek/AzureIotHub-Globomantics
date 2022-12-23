using Globomantics.BandAgent;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Reflection;

// Hentes fra User Secrets. Findes som primary connectionstring for den enkelte device

var configuration = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

Console.WriteLine("Initializing Band Agent...");

var device = DeviceClient.CreateFromConnectionString(configuration["DeviceConnectionString"]);

await device.OpenAsync();

Console.WriteLine("Device is connected!");

#region Sending Device messages to Cloud
for (int i = 0; i < 10; i++)
{
    var telemetry = new Telemetry
    {
        Message = "Sending complex object...",
        StatusCode = i
    };

    string telemetryJson = JsonSerializer.Serialize(telemetry, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    var message = new Message(Encoding.ASCII.GetBytes(telemetryJson));

    await device.SendEventAsync(message);

    Console.WriteLine("Message sent to the cloud!");

    Thread.Sleep(2000);
}
#endregion

#region Sending Device Twin messages to Cloud
// Kan kontrolleres vha. Azure IoT Explorer eller i Portal
//var twinProperties = new TwinCollection();
//twinProperties["connectionType"] = "wi-fi";
//twinProperties["connectionStrength"] = "full";

//await device.UpdateReportedPropertiesAsync(twinProperties);
#endregion



Console.WriteLine("Press any key to exit...");
Console.ReadKey();


