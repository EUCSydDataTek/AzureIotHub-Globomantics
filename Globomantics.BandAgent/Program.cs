using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Common;

// Send messages from the cloud to your device with IoT Hub (.NET) https://learn.microsoft.com/en-us/azure/iot-hub/iot-hub-csharp-csharp-c2d)

// Connectionstring hentes fra User Secrets. Findes som primary connectionstring for den enkelte device

var configuration = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

Console.WriteLine("Initializing Band Agent...");

DeviceClient device = DeviceClient.CreateFromConnectionString(configuration["DeviceConnectionString"]);
await device.SendEventAsync(new Message(Encoding.ASCII.GetBytes("Device is connected!")));

TwinCollection _reportedProperties = new TwinCollection();

Console.WriteLine($"Device {device.ProductInfo} is connected!");

await UpdateTwin(device);                                                          // #7 Added

await device.SetDesiredPropertyUpdateCallbackAsync(UpdateProperties, null);        // #7 Added

Console.WriteLine("Press a key to perform an action:");
Console.WriteLine("q: quits");
Console.WriteLine("h: send happy feedback");
Console.WriteLine("u: send unhappy feedback");
Console.WriteLine("e: request emergency help");

Random random = new Random();
bool quitRequested = false;

while (!quitRequested)
{
    Console.Write("Action? ");
    char input = Console.ReadKey().KeyChar;
    Console.WriteLine();

    StatusType status = StatusType.NotSpecified;
    int latitude = random.Next(0, 100);
    int longitude = random.Next(0, 100);

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

    Telemetry telemetry = new Telemetry
    {
        Latitude = latitude,
        Longitude = longitude,
        Status = status
    };

    string payload = JsonSerializer.Serialize(telemetry, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    Message message = new Message(Encoding.ASCII.GetBytes(payload));
    await device.SendEventAsync(message);
    Console.WriteLine("Message sent!");
}

Console.WriteLine("Disconnecting...");

#region DEVICE TWIN PROPERTIES
async Task UpdateTwin(DeviceClient device)
{
    _reportedProperties["firmwareVersion"] = "2.0";
    _reportedProperties["firmwareUpdateStatus"] = "n/a";

    await device.UpdateReportedPropertiesAsync(_reportedProperties);
}

async Task UpdateProperties(TwinCollection desiredProperties, object userContext)
{
    var currentFirmwareVersion = (string)_reportedProperties["firmwareVersion"];
    string desiredFirmwareVersion = (string)desiredProperties["firmwareVersion"];

    if (currentFirmwareVersion != desiredFirmwareVersion)
    {
        Console.WriteLine($"Firmware update requested.  Current version: '{currentFirmwareVersion}', " +
                          $"requested version: '{desiredFirmwareVersion}'");

        await ApplyFirmwareUpdate(desiredFirmwareVersion);
    }
}

 async Task ApplyFirmwareUpdate(string targetVersion)
{
    Console.WriteLine("Beginning firmware update...");

    _reportedProperties["firmwareUpdateStatus"] = $"Downloading zip file for firmware {targetVersion}...";
    await device.UpdateReportedPropertiesAsync(_reportedProperties);
    Thread.Sleep(5000);

    _reportedProperties["firmwareUpdateStatus"] = "Unzipping package...";
    await device.UpdateReportedPropertiesAsync(_reportedProperties);
    Thread.Sleep(5000);

    _reportedProperties["firmwareUpdateStatus"] = "Applying update...";
    await device.UpdateReportedPropertiesAsync(_reportedProperties);
    Thread.Sleep(5000);

    Console.WriteLine("Firmware update complete!");

    _reportedProperties["firmwareUpdateStatus"] = "n/a";
    _reportedProperties["firmwareVersion"] = targetVersion;
    await device.UpdateReportedPropertiesAsync(_reportedProperties);
}
#endregion

