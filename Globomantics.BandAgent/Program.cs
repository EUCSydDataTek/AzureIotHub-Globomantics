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

var device = DeviceClient.CreateFromConnectionString(configuration["DeviceConnectionString"]);

await device.OpenAsync();

Task receiveEventsTask = ReceiveEventsTask(device);     // Added #1

await device.SetMethodHandlerAsync("showMessage", ShowMessage, null); // Added #4

await device.SetMethodDefaultHandlerAsync(OtherDeviceMethod, null); // Added #4

Console.WriteLine("Device is connected!");

await UpdateTwin(device);

//await _device.SetDesiredPropertyUpdateCallbackAsync(UpdateProperties, null);

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
    twinProperties["connectionType"] = "wi-fi";
    twinProperties["connectionStrength"] = "full";

    await device.UpdateReportedPropertiesAsync(twinProperties);
}

static async Task ReceiveEventsTask(DeviceClient device)    // Added #1
{
    while (true)
    {
        Message message = await device.ReceiveAsync();

        if (message == null) continue;

        string payload = Encoding.ASCII.GetString(message.GetBytes());
        Console.WriteLine($"Received message from cloud: {payload}");

        //await device.RejectAsync(message);
        //await device.AbandonAsync(message);
        await device.CompleteAsync(message);
    }
}

#region DIRECT METHODS
static Task<MethodResponse> ShowMessage(MethodRequest methodRequest, object userContext)
{
    Console.WriteLine("*** MESSAGE RECEIVED ***");
    Console.WriteLine(methodRequest.DataAsJson);

    var responsePayload = Encoding.ASCII.GetBytes("{\"response\": \"Message shown!\"}");

    return Task.FromResult(new MethodResponse(responsePayload, 200));
}


static Task<MethodResponse> OtherDeviceMethod(MethodRequest methodRequest, object userContext)
{
    Console.WriteLine("****OTHER DEVICE METHOD CALLED****");
    Console.WriteLine($"Method: {methodRequest.Name}");
    Console.WriteLine($"Payload: {methodRequest.DataAsJson}");

    var responsePayload = Encoding.ASCII.GetBytes("{\"response\": \"The method is not implemented!\"}");

    return Task.FromResult(new MethodResponse(responsePayload, 404));
}
#endregion

//static Task UpdateProperties(
//    TwinCollection desiredProperties,
//    object userContext)
//{
//    var currentFirmwareVersion = (string)_reportedProperties["firmwareVersion"];
//    var desiredFirmwareVersion = (string)desiredProperties["firmwareVersion"];

//    if (currentFirmwareVersion != desiredFirmwareVersion)
//    {
//        Console.WriteLine($"Firmware update requested.  Current version: '{currentFirmwareVersion}', " +
//                          $"requested version: '{desiredFirmwareVersion}'");

//        ApplyFirmwareUpdate(desiredFirmwareVersion);
//    }

//    return Task.CompletedTask;
//}

//static async Task ApplyFirmwareUpdate(string targetVersion)
//{
//    Console.WriteLine("Beginning firmware update...");

//    _reportedProperties["firmwareUpdateStatus"] =
//        $"Downloading zip file for firmware {targetVersion}...";
//    await _device.UpdateReportedPropertiesAsync(_reportedProperties);
//    Thread.Sleep(5000);

//    _reportedProperties["firmwareUpdateStatus"] = "Unzipping package...";
//    await _device.UpdateReportedPropertiesAsync(_reportedProperties);
//    Thread.Sleep(5000);

//    _reportedProperties["firmwareUpdateStatus"] = "Applying update...";
//    await _device.UpdateReportedPropertiesAsync(_reportedProperties);
//    Thread.Sleep(5000);

//    Console.WriteLine("Firmware update complete!");

//    _reportedProperties["firmwareUpdateStatus"] = "n/a";
//    _reportedProperties["firmwareVersion"] = targetVersion;
//    await _device.UpdateReportedPropertiesAsync(_reportedProperties);
//}

//private static async Task UpdateTwin(DeviceClient device)
//{
//    _reportedProperties = new TwinCollection();
//    _reportedProperties["firmwareVersion"] = "1.0";
//    _reportedProperties["firmwareUpdateStatus"] = "n/a";

//    await device.UpdateReportedPropertiesAsync(_reportedProperties);
//}
