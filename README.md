# Globomantics

## 1. BandAgent

Device: `my-device`

Connectionstring findes som *Primary Connectionstring* for my-device devicen og er anbragt i *User Secrets*.

Testes med Azure IoT Explorer, som monitorerer *Event Hub*.

Menu i BandAgent:

> q: quits
> 
> h: send happy feedback
> 
> u: send unhappy feedback
> 
> e: request emergency help

#### Device Twin
Indkommentér koden i `BandAgent` klassen, som opdaterer Device Twin:

```csharp
await UpdateTwin(device);
```
Benyt Azure IoT Explorer til at se ændringen i Device Twin.

&nbsp;

## 2. MessageProcessor

Uden deserilization af Json og med `return Task.CompletedTask` i slutningen 
af klassen `LoggingEventProcessor` og metoden `ProcessEventsAsync()` :


1. Sæt **Message Processor** til Startup project.
1. Start **MessageProcessor** og bemærk at der sikkert allerede ligger en masse messages.
1. Send nu 10 messages fra VSCode.
1. Stop og start igen og se at alle gamle samt de 10 nye messages stadig ligger i EventHub.
1. Udskift `Task.CompletedTask` med `return context.CheckpointAsync()`
1. Start MessageProcessor igen - alle messages vises stadig
1. Start MessageProcessor igen - og denne gang flyttes checkpoint korrekt og Eventhub er tom!

&nbsp;

## 3. Horizontal scaling


1. Tryk Ctrl + F5 for at starte første instance af Message Processor.

2. Tryk Ctrl + F5 for at starte endnu en instance af Message Processor. 
Bemærk at den tager den ene partition, som den første host så giver slip på (LeaseLost).
De er nu balancerede og hver host tager sig af sin egen partition.

3. Tryk Ctrl + F5 for at starte en 3. instance af Message Processor. Bemærk at den ikke starter op.
Vi har nu flere processors end partitions (free hub = 2 partitioner).

4. Stop nu den første processor med ENTER og vent.
På et tidspunkt (op til 1 minut) overtager processor #3 partition 0 og partition #3 overtager partition 1.
Det er horizontal scaling!

Bemærk: luk ikke processors ned ved blot at lukke vinduet! Så laves der ikke cleanup og leases sættes ikke fri
før der er gået lidt tid!

&nbsp;

## 4. Device-to-Cloud Messages

Indkommentér Deserilization-koden ind i `LoggingEventProcessor` klassen.

MessageProcessor og BandAgent sættes til Multiple Startup. Når der sendes forskellige meddelelser fra BandAgent bliver de hentet fra EventHub, deserilizeret til Telemetry-klassen og vist vha. MessageProcessor.