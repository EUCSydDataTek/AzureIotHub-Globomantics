using Common;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text;
using System.Text.Json;

// Device: my-device
// Connectionstring hentes fra User Secrets. Findes som primary connectionstring for den enkelte device

public class Program
{
    public static async Task Main()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        Console.WriteLine("Initializing Band Agent...");

        DeviceClient device = DeviceClient.CreateFromConnectionString(configuration["DeviceConnectionString"]);

        await device.SetReceiveMessageHandlerAsync(OnC2dMessageReceiverAsync, device);      // C2D Message handler
        await device.SetMethodHandlerAsync("showMessage", ShowMessage, null);               // Direct Method handler
        await device.SetMethodDefaultHandlerAsync(OtherDeviceMethod, null);                 // All other Direct Methods handler

        await device.SendEventAsync(new Message(Encoding.ASCII.GetBytes("Device is connected!")));
        Console.WriteLine("Device is connected!");

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
    }


    #region CLOUD-TO-DEVICE MESSAGES (C2D)
    private static async Task OnC2dMessageReceiverAsync(Message receivedMessage, object userContext)
    {
        try
        {
            PrintMessage(receivedMessage);
            await ((DeviceClient)userContext).CompleteAsync(receivedMessage);
        }
        finally
        {
            receivedMessage.Dispose();
        }
    }

    private static void PrintMessage(Message receivedMessage)
    {
        string messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
        var formattedMessage = new StringBuilder($"Received message: [{messageData}]\n");

        // User set application properties can be retrieved from the Message.Properties dictionary.
        foreach (KeyValuePair<string, string> prop in receivedMessage.Properties)
        {
            formattedMessage.AppendLine($"\tProperty: key={prop.Key}, value={prop.Value}");
        }
        // System properties can be accessed using their respective accessors, e.g. ContentType.
        //formattedMessage.AppendLine($"\tContent type: {receivedMessage.ContentType}");

        Console.WriteLine($"{DateTime.Now}> {formattedMessage}");
    }
    #endregion

    #region DIRECT METHODS
    public static Task<MethodResponse> ShowMessage(MethodRequest methodRequest, object userContext)
    {
        Console.WriteLine("*** DIRECT MESSAGE RECEIVED ***");
        Console.WriteLine(methodRequest.DataAsJson);

        var responsePayload = Encoding.ASCII.GetBytes("{\"response\": \"Message shown!\"}");

        return Task.FromResult(new MethodResponse(responsePayload, 200));
    }


    public static Task<MethodResponse> OtherDeviceMethod(MethodRequest methodRequest, object userContext)
    {
        Console.WriteLine("****OTHER DEVICE METHOD CALLED****");
        Console.WriteLine($"Method: {methodRequest.Name}");
        Console.WriteLine($"Payload: {methodRequest.DataAsJson}");

        var responsePayload = Encoding.ASCII.GetBytes("{\"response\": \"The method is not implemented!\"}");

        return Task.FromResult(new MethodResponse(responsePayload, 404));
    }
    #endregion
}