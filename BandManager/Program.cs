using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

// Connectionstring hentes fra User Secrets. Findes under IoT-Hub | Shared access policies | Service | Connection string - primary key

var configuration = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

Console.WriteLine("Initializing Band Agent...");

var serviceClient = ServiceClient.CreateFromConnectionString(configuration["ServiceConnectionString"]);

var registryManager = RegistryManager.CreateFromConnectionString(configuration["ServiceConnectionString"]);          // #7 Added   

while (true)
{
    Console.WriteLine("Which device do you wish to update? ");
    Console.Write("> ");
    string? deviceId = Console.ReadLine();               

    await UpdateDeviceFirmware(registryManager, deviceId);                                                          // #7 Added
}

async Task SendCloudToDeviceMessage(ServiceClient serviceClient, string? deviceId)
{
    Console.WriteLine("What message payload do you want to send? ");
    Console.Write("> ");
    string? payload = Console.ReadLine();

    Message commandMessage = new Message(Encoding.ASCII.GetBytes(payload!));

    commandMessage.MessageId = Guid.NewGuid().ToString();
    commandMessage.Ack = DeliveryAcknowledgement.Full;
    commandMessage.ExpiryTimeUtc = DateTime.UtcNow.AddSeconds(10);

    await serviceClient.SendAsync(deviceId, commandMessage);
}

 static async Task UpdateDeviceFirmware(RegistryManager registryManager, string deviceId)                       // #7 Added
{
    Twin deviceTwin = await registryManager.GetTwinAsync(deviceId);

    var twinPatch = new
    {
        properties = new
        {
            desired = new
            {
                firmwareVersion = "2.0"
            }
        }
    };

    string twinPatchJson = JsonConvert.SerializeObject(twinPatch);

    await registryManager.UpdateTwinAsync(deviceId, twinPatchJson, deviceTwin.ETag);

    Console.WriteLine($"Firmware update sent to device '{deviceId}'...");

    while (true)
    {
        Thread.Sleep(1000);

        deviceTwin = await registryManager.GetTwinAsync(deviceId);

        Console.WriteLine($"Firmware update status: {deviceTwin.Properties.Reported["firmwareUpdateStatus"]}");

        if (deviceTwin.Properties.Reported["firmwareVersion"] == "2.0")
        {
            Console.WriteLine("Firmware update complete!");
            break;
        }
    }
}
