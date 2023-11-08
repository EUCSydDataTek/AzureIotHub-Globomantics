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

1. Indkomment�r metoden `ShowMessage()`.

2. Start *BandAgent* og benyt *IoT Hub Explorer* til at kalde **showMessage** med en Payload-message, som en Direct Method p� `my-device-id`.

3. Pr�v ogs� at kalde **showUnknownMessage** og bem�rk en HTTP 501

4. Indkomment�r metoden `OtherDeviceMethod()`.

5. Start *BandAgent* og benyt *IoT Hub Explorer* til at kalde **showUnknownMessage** med en Payload-message, som en Direct Method p� `my-device-id`.

&nbsp;

## 5. Call Direct Method from BandManager

1. Indkomment�r metoden `CallDirectMethod()` i BandManager.
2. Start b�de BandAgent og BandMangeger og send til `my-device-id`. Virker desv�rre ikke!

&nbsp;

## 6. BandAgent Device Twins

Der er tilf�jet nogle update metoder i BandAgent.

1. Opret en helt ny device og opdater User Secrets.
2. Vha. Azure IoT Explorer vises Device Twin, som mangler property for `"firmwareVersion"`
3. Singlestep BandAgent fra metoden `UpdateTwin()` og kontroll�r bagefter med Azure IoT Explorer at  property for `"firmwareVersion"` nu findes i Device Twin
4. S�t breakpoint ved metoden `UpdateProperties()` og �ndre firmwareVersion til **2.0** og Save. F�lg hvordan metoden opdaterer i BandAgent
5. Bem�rk ogs� hvordan `"$lastUpdated"` og `"$lastUpdatedVersion"` �ndrer sig for hver gang der laves en opdatering af Twin properties

&nbsp;

## 7. BandManager Device Twins

1. I Azure IoT Hub og *Shared access policies* udvides *service Policy* med **Registry write**.

2. I BandAgent abonneres p� metoden `UpdateProperties`, som igen kalder �pplyFirmwareUpdate()`hvis der �nskes en h�jere version.
   I BandManager opdateres Device Twin med en ny h�jere firmwareVersion og dermed opdateres BandAgent.

3. B�de BandAgent og BandManager k�res nu. Skriv navnet p� devicen i BandManager og processen kan f�lges i begge ender.
