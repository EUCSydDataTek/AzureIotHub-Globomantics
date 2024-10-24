# 2.Cloud-to-Device-Messaging

## 1. Receiving Messages on a Device (BandAgent) (demo af afsnit 17)

Følgende metode tilføjes **BandAgent**:

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

## 2. Sending Messages from the BandManager

En ny ConsoleApp kaldet **BandManager** er oprettet.

I første omgang er kun metoden `SendCloudToDeviceMessage()` aktiv.

Test multiple startup af både *BandAgent* og *BandManager* projekterne og send en meddelelse til `my-device`. Den skal dukke op i BandAgent.

Luk begge apps, start kun *BandManager* og send en ny meddelelse - men denne gang ved vi ikke om den er modtaget!

&nbsp;

## 3. Using Message Feedback

Indkommentér metoden `ReceiveFeedback()` i `BandManager`.
Start både `BandAgent` og `BandManager` projekterne og test følgende:

- at meddelelsen bliver modtaget
- at meddelelsen ikke bliver modtaget og timer ud

&nbsp;

## 4. Receiving Direct Method Calls

1. Bemærk metoden `ShowMessage()` i *BandAgent*.

2. Start *BandAgent* og benyt *IoT Hub Explorer* til at kalde **showMessage** med en Payload-message, som en Direct Method på `my-device`.

3. Prøv også at kalde `showUnknownMessage()` og bemærk en `HTTP 501`

4. Indkommentér metoden `OtherDeviceMethod()`.

5. Start *BandAgent* og benyt *IoT Hub Explorer* til at kalde **showUnknownMessage** med en Payload-message, som en Direct Method på `my-device`.

&nbsp;

## 5. Call Direct Method from BandManager

1. Indkommentér metoden `CallDirectMethod()` i BandManager.
2. Start både *BandAgent* og *BandManager* og send til `my-device` uden at skrive en message.




