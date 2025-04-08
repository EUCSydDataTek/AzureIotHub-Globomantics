using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;

namespace MessageProcessor;

public sealed class LoggingEventProcessor : IDisposable
{
    private readonly EventProcessorClient _processor;
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public LoggingEventProcessor(EventProcessorClient processor)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        
        _processor.ProcessEventAsync += ProcessEventsAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
        _processor.PartitionInitializingAsync += OpenAsync;
        _processor.PartitionClosingAsync += CloseAsync;
    }
    
    private static Task OpenAsync(PartitionInitializingEventArgs args)
    {
        Console.WriteLine("LoggingEventProcessor opened, processing partition: " +
                          $"'{args.PartitionId}'");
        
        return Task.CompletedTask;
    }

    private static Task CloseAsync(PartitionClosingEventArgs args)
    {
        Console.WriteLine("LoggingEventProcessor closing, partition: " +
                          $"'{args.PartitionId}', reason: '{args.Reason}'.");
                          
        return Task.CompletedTask;
    }
    
    private static Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine("LoggingEventProcessor error, partition: " +
                          $"{args.PartitionId}, error: {args.Exception.Message}");
        
        return Task.CompletedTask;
    }
    
    private static Task ProcessEventsAsync(ProcessEventArgs args)
    {
        Console.WriteLine($"Event received on partition '{args.Partition.PartitionId}'.");

        try
        {
            EventData? eventData = args.Data;
            string payload = Encoding.ASCII.GetString(eventData.Body.ToArray(),0, eventData.Body.Length);
            object? deviceId = eventData.SystemProperties["iothub-connection-device-id"];
            
            Console.WriteLine($"Message received on partition '{args.Partition.PartitionId}', " +
                              $"device ID: '{deviceId}', " +
                              $"payload: '{payload}'");

            // 4. Device-to-Cloud Messages
            // Telemetry? telemetry = JsonSerializer.Deserialize<Telemetry>(payload, JsonSerializerOptions);
            // if (telemetry?.Status == StatusType.Emergency)
            // {
            //     Console.WriteLine($"Guest requires emergency assistance! Device ID: {deviceId}");
            //     SendFirstRespondersTo(telemetry.Latitude, telemetry.Longitude);
            // }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return Task.CompletedTask;        // 2. MessageProcessor
        //return args.UpdateCheckpointAsync();
    }
    
    private static void SendFirstRespondersTo(decimal latitude, decimal longitude)
    {
        Console.WriteLine($"**First responders dispatched to ({latitude}, {longitude})!**");
    }

    public void Dispose()
    {
        _processor.ProcessEventAsync -= ProcessEventsAsync;
        _processor.ProcessErrorAsync -= ProcessErrorAsync;
        _processor.PartitionInitializingAsync -= OpenAsync;
        _processor.PartitionClosingAsync -= CloseAsync;
    }
}