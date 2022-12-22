using Globomantics.BandAgent;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System.Text;
using System.Text.Json;

// Findes som primary connectionstring for den enkelte device
const string DeviceConnectionString = "HostName=IoTHubECR.azure-devices.net;DeviceId=my-device-id;SharedAccessKey=FPR92/gYb0yjaIyw8iTStews4CbvWeI1qV2M2MnLaKc=";


Console.WriteLine("Initializing Band Agent...");

var device = DeviceClient.CreateFromConnectionString(DeviceConnectionString);

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
//twinProperties["connection-type"] = "wi-fi";
//twinProperties["connection-strength"] = "full";

//await device.UpdateReportedPropertiesAsync(twinProperties);
#endregion



Console.WriteLine("Press any key to exit...");
Console.ReadKey();


