using Common;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Newtonsoft.Json;
using System.Text;

public class LoggingEventProcessor : IEventProcessor
{
    public Task OpenAsync(PartitionContext context)
    {
        Console.WriteLine("LoggingEventProcessor opened, processing partition: " +
                          $"'{context.PartitionId}'");
        return Task.CompletedTask;
    }

    public Task CloseAsync(PartitionContext context, CloseReason reason)
    {
        Console.WriteLine("LoggingEventProcessor closing, partition: " +
                          $"'{context.PartitionId}', reason: '{reason}'.");
        return Task.CompletedTask;
    }

    public Task ProcessErrorAsync(PartitionContext context, Exception error)
    {
        Console.WriteLine("LoggingEventProcessor error, partition: " +
                          $"{context.PartitionId}, error: {error.Message}");
        return Task.CompletedTask;
    }

    public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
    {
        Console.WriteLine($"Batch of events received on partition '{context.PartitionId}'.");

        try
        {
            foreach (var eventData in messages)
            {
                var payload = Encoding.ASCII.GetString(eventData.Body.Array!,
                    eventData.Body.Offset,
                    eventData.Body.Count);

                var deviceId = eventData.SystemProperties["iothub-connection-device-id"];

                Console.WriteLine($"Message received on partition '{context.PartitionId}', " +
                                  $"device ID: '{deviceId}', " +
                                  $"payload: '{payload}'");

                // 4. Device-to-Cloud Messages
                //var telemetry = JsonConvert.DeserializeObject<Telemetry>(payload);

                //if (telemetry.Status == StatusType.Emergency)
                //{
                //    Console.WriteLine($"Guest requires emergency assistance! Device ID: {deviceId}");
                //    SendFirstRespondersTo(telemetry.Latitude, telemetry.Longitude);
                //}
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return Task.CompletedTask;        // 2. MessageProcessor
        //return context.CheckpointAsync();
    }

    private void SendFirstRespondersTo(decimal latitude, decimal longitude)
    {
        Console.WriteLine($"**First responders dispatched to ({latitude}, {longitude})!**");
    }
}
