using Microsoft.Extensions.Configuration;
using System.Reflection;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using MessageProcessor;

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

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

const string storageContainerName = "message-processor-host";
const string consumerGroupName = EventHubConsumerClient.DefaultConsumerGroupName;

// Create a BlobContainerClient to interact with the storage container
BlobContainerClient storageClient = new(
    configuration["storageConnectionString"],
    storageContainerName);

// Create the EventProcessorClient
EventProcessorClient processor = new(
    storageClient,
    consumerGroupName,
    configuration["IotHubConnectionString"],
    configuration["HubName"]);

// Create the LoggingEventProcessor
using LoggingEventProcessor loggingEventProcessor = new(processor);

await processor.StartProcessingAsync();

Console.WriteLine("Event processor started, press enter to exit...");
Console.ReadLine();

await processor.StopProcessingAsync();