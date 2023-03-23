using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using System.Reflection;

/*   HubName og IoTHubConnectionstring findes her: Hub settings | Built-in endpoints | Event Hub-compatible name and endpoint
     Inden projektet kan køres, skal der oprettes en ny Blob Container, der kaldes message-processor-host og gemmer checkpoints for EventHub.
     StorageConnectionString og Name findes Her: iothubecrstorage | Access keys

     IoTHubConnectionString, StorageConnectionString og HubName hentes fra User Secrets med følgende format:
     {
      "IotHubConnectionString": "EVENT_HUB_COMPATIBLE_ENDPOINT",
      "StorageConnectionString": "STORAGE_CONNECTION_STRING",
      "HubName": "YOUR_HUBNAME"
     }
*/

var configuration = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

var storageContainerName = "message-processor-host";
var consumerGroupName = PartitionReceiver.DefaultConsumerGroupName;

var processor = new EventProcessorHost(
    configuration["HubName"],
    consumerGroupName,
    configuration["IotHubConnectionString"],
    configuration["storageConnectionString"],
    storageContainerName);

await processor.RegisterEventProcessorAsync<LoggingEventProcessor>();

Console.WriteLine("Event processor started, press enter to exit...");
Console.ReadLine();