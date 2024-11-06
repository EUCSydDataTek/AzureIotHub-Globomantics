# 3. Device-Twins

## 6. BandAgent Device Twins

Der er tilføjet nogle update metoder i BandAgent.

1. Device: `my-device`
2. Vha. Azure IoT Explorer vises Device Twin, som første gang mangler property for `firmwareVersion`
3. Start BandAgent og kontrollér med Azure IoT Explorer at  property for `"firmwareVersion"` nu findes i Device Twin
4. I Azure IoT Explorer ændres firmwareVersion til **2.0** og tryk på Save. Følg hvordan metoden opdaterer i BandAgent.
5. Bemærk også hvordan `"$lastUpdated"` og `"$lastUpdatedVersion"` ændrer sig for hver gang der laves en opdatering af Twin properties

&nbsp;

## 7. BandManager Device Twins

1. I Azure IoT Hub og *Shared access policies* udvides *service Policy* med **Registry write**.

2. I BandAgent abonneres på metoden `UpdateProperties`, som igen kalder ÀpplyFirmwareUpdate()`hvis der ønskes en højere version.
   I BandManager opdateres Device Twin med en ny højere firmwareVersion og dermed opdateres BandAgent.

3. Både BandAgent og BandManager køres nu. Skriv navnet på devicen i BandManager og processen kan følges i begge ender.
