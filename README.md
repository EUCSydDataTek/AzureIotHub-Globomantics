# 2.Cloud-to-Device-Messaging

## 1. Receiving Messages on a Device (BandAgent)

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

## 2. Sending Messages from the Cloud

En ny ConsoleApp kaldet **BandManager** er oprettet.

I første omgang er kun metoden `SendCloudToDeviceMessage()` aktive.

Test multiple startup af både BandAgent og BandManager projekterne og send en meddelelse til `my-device-id`. Den skal dukke op i BandAgent.

Derefter lukkes for BandAgent og en ny meddelelse sendes - men her ved vi ikke om den er modtaget!

&nbsp;

## 3. Using Message Feedback

Indkommentér metoden ReceiveFeedback() og test:

- at meddelelsen bliver modtaget
- at meddelelsen ikke bliver modtaget og timer ud

&nbsp;

## 4. Receiving Direct Method Calls

1. Indkommentér metoden `ShowMessage()`.

2. Start *BandAgent* og benyt *IoT Hub Explorer* til at kalde **showMessage** med en Payload-message, som en Direct Method på `my-device-id`.

3. Prøv også at kalde **showUnknownMessage** og bemærk en HTTP 501

4. Indkommentér metoden `OtherDeviceMethod()`.

5. Start *BandAgent* og benyt *IoT Hub Explorer* til at kalde **showUnknownMessage** med en Payload-message, som en Direct Method på `my-device-id`.

&nbsp;

## 5. Call Direct Method from BandManager

1. Indkommentér metoden `CallDirectMethod()` i BandManager.
2. Start både BandAgent og BandMangeger og send til `my-device-id`. Virker desværre ikke!

&nbsp;

## 6. BandAgent Device Twins

Der er tilføjet nogle update metoder i BandAgent.

1. Opret en helt ny device og opdater User Secrets.
2. Vha. Azure IoT Explorer vises Device Twin, som mangler property for `"firmwareVersion"`
3. Singlestep BandAgent fra metoden `UpdateTwin()` og kontrollér bagefter med Azure IoT Explorer at  property for `"firmwareVersion"` nu findes i Device Twin
4. Sæt breakpoint ved metoden `UpdateProperties()` og ændre firmwareVersion til **2.0** og Save. Følg hvordan metoden opdaterer i BandAgent
5. Bemærk også hvordan `"$lastUpdated"` og `"$lastUpdatedVersion"` ændrer sig for hver gang der laves en opdatering af Twin properties

&nbsp;

## 7. BandManager Device Twins

1. I Azure IoT Hub og *Shared access policies* udvides *service Policy* med **Registry write**.

2. I BandAgent abonneres på metoden `UpdateProperties`, som igen kalder ÀpplyFirmwareUpdate()`hvis der ønskes en højere version.
   I BandManager opdateres Device Twin med en ny højere firmwareVersion og dermed opdateres BandAgent.

3. Både BandAgent og BandManager køres nu. Skriv navnet på devicen i BandManager og processen kan følges i begge ender.
