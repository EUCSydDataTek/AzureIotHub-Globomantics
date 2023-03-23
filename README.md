# Globomantics

## 1. BandAgent

Device: `my-device-id`

Connectionstring findes som *Primary Connectionstring* for my-device-id devicen og er anbragt i *User Secrets*.

Testes med Azure IoT Explorer, som monitorerer *Event Hub*.

Menu i BandAgent:

> q: quits
> 
> h: send happy feedback
> 
> u: send unhappy feedback
> 
> e: request emergency help

&nbsp;

## 2. MessageProcessor

Uden deserilization af Json og med `return Task.CompletedTask`:

1. Start MessageProcessor og send 10 messages fra VSCode.
2. Stop og start igen og se at alle 10 messages stadig ligger i EventHub.
3. Udskift `Task.CompletedTask` med `return context.CheckpointAsync()`
4. Start MessageProcessor igen - alle 10 vises stadig
5. Start MessageProcessor igen - og denne gang flyttes checkpoint korrekt.

&nbsp;

## 3. Horizontal scaling

S�t Message Processor til Startup project.

1. Tryk Ctrl + F5 for at starte f�rste instance af Message Processor.

2. Tryk Ctrl + F5 for at starte endnu en instance af Message Processor. 
Bem�rk at den tager den ene partition, som den f�rste host s� giver slip p� (LeaseLost).
De er nu balancerede og hver host tager sig af sin egen partition.

3. Tryk Ctrl + F5 for at starte en 3. instance af Message Processor. Bem�rk at den ikke starter op.
Vi har nu flere processors end partitions (free hub = 2 partitioner).

4. Stop nu den f�rste processor med ENTER og vent.
P� et tidspunkt (op til 1 minut) overtager processor #3 partition 0 og partition #3 overtager partition 1.
Det er horizontal scaling!

Bem�rk: luk ikke processors ned ved blot at lukke vinduet! S� laves der ikke cleanup og leases s�ttes ikke fri
f�r der er g�et lidt tid!

&nbsp;

## 4. Device-to-Cloud Messages

Indkomment�r Deserilization-koden ind i `LoggingEventProcessor` klassen.

MessageProcessor og BandAgent s�ttes til Multiple Startup. N�r der sendes forskellige meddelelser fra BandAgent bliver de hentet fra EventHub, deserilizeret til Telemetry-klassen og vist vha. MessageProcessor.