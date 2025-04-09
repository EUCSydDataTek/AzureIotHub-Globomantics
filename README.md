# 2.Cloud-to-Device-Messaging
Ref: [Send cloud-to-device messages](https://learn.microsoft.com/en-us/azure/iot-hub/how-to-cloud-to-device-messaging?pivots=programming-language-csharp#send-cloud-to-device-messages)

Ref: https://github.com/Azure/azure-iot-sdk-csharp/blob/main/iothub/device/samples/getting%20started/MessageReceiveSample/MessageReceiveSample.cs

## 1. Receiving Messages on a Device (BandAgent) (demo af afsnit 17)

En callback metode registreres i **BandAgent**:

```csharp
await device.SetReceiveMessageHandlerAsync(OnC2dMessageReceiverAsync, device);      // C2D Message handler
```

Og metoden `OnC2dMessageReceiverAsync` kaldes, når en C2D Message modtages:

```csharp
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
 ```

Test fra VSCode ved at sende en C2D Message to Device. Prøv også at tilføje parametre til beskeden.

&nbsp;

## 2. Sending Messages from the BandManager

En ny ConsoleApp kaldet **BandManager** er oprettet.

I første omgang er kun metoden `SendCloudToDeviceMessage()` aktiv.

Test multiple startup af både *BandAgent* og *BandManager* projekterne og send en meddelelse til `my-device`. Den skal dukke op i BandAgent.

Derefter indkobles ReceiveFeedback() i linje 16. Start nu kun BandManager op og send message til BandAgent - vent 10 sekunder og se Feedback med status code Expired.

&nbsp;


## 3. Receiving Direct Method Calls

1. Bemærk metoden `ShowMessage()` i *BandAgent*.

2. Start *BandAgent* og benyt *IoT Hub Explorer* til at kalde **showMessage** med en Payload-message (bemærk case-sensitive), som en Direct Method på `my-device`.

3. Bemærk metoden `OtherDeviceMethod()` i BandAgent.

4. Benyt *IoT Hub Explorer* til at kalde **showUnknownMessage** med en Payload-message, som en Direct Method på `my-device` og se at nu kaldes OtherDeviceMethod() i stedet for ShowMessage().

&nbsp;

## 4. Call Direct Method from BandManager

1. Indkommentér metoden `CallDirectMethod()` i BandManager.
2. Start både *BandAgent* og *BandManager* og send til `my-device` uden at skrive en message.




