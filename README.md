# 2.Cloud-to-Device-Messaging

## 1. Receiving Messages on a Device (BandAgent)

F�lgende metode tilf�jes **BandAgent**:

```csharp
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
```

Test fra VSCode ved at sende en C2D Message to Device.

&nbsp;

## 2. Sending Messages from the Cloud

En ny ConsoleApp kaldet **BandManager** er oprettet.

I f�rste omgang er kun metoden `SendCloudToDeviceMessage()` aktive.

Test multiple startup af b�de BandAgent og BandManager projekterne og send en meddelelse til `my-device-id`. Den skal dukke op i BandAgent.

Derefter lukkes for BandAgent og en ny meddelelse sendes - men her ved vi ikke om den er modtaget!

&nbsp;

## 3. Using Message Feedback

Indkomment�r metoden ReceiveFeedback() og test:

- at meddelelsen bliver modtaget
- at meddelelsen ikke bliver modtaget og timer ud

&nbsp;

## 4. Receiving Direct Method Calls

Indkomment�r metoden ShowMessage().

Start *BandAgent* og benyt *IoT Hub Explorer* til at kalde **showMessage** som en Direct Method p� `my-device-id`.