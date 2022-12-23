using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using System.Reflection;

// HubName og IoTHubConnectionstring findes her: Hub settings | Buil-in endpoints | Event Hub-compatible name and endpoint
// Der er oprettet en ny Blob Container, der kaldes message-processor-host
// StorageConnectionString og Name findes Her: iothubecrstorage | Access keys
// IoTHubConnectionString og StorageConnectionString hentes fra User Secrets med følgende format:
//{
//  "IotHubConnectionString": "EVENT_HUB_COMPATIBLE_ENDPOINT",
//  "StorageConnectionString": "STORAGE_CONNECTION_STRING"
//}

var configuration = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

var hubName = "iothub-ehub-iothubecr-8750402-cf4acd3ffb";
var storageContainerName = "message-processor-host";
var consumerGroupName = PartitionReceiver.DefaultConsumerGroupName;

var processor = new EventProcessorHost(
    hubName,
    consumerGroupName,
    configuration["IotHubConnectionString"],
    configuration["storageConnectionString"],
    storageContainerName);

await processor.RegisterEventProcessorAsync<LoggingEventProcessor>();

Console.WriteLine("Event processor started, press enter to exit...");
Console.ReadLine();