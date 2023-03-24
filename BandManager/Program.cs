﻿
// Nået til 6. Direct Message Overview https://app.pluralsight.com/course-player?clipId=f1215bae-9cdb-4900-9a3d-fad64b2ae1cd)

// Send a cloud-to-device message (https://learn.microsoft.com/en-us/azure/iot-hub/iot-hub-csharp-csharp-c2d#send-a-cloud-to-device-message)

using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text;

// Connectionstring hentes fra User Secrets. Findes under IoT-Hub | Shared access policies | Service | Connection string - primary key

var configuration = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

Console.WriteLine("Initializing Band Agent...");

var serviceClient = ServiceClient.CreateFromConnectionString(configuration["ServiceConnectionString"]);

//var feedbackTask = ReceiveFeedback(serviceClient);    // 1. Uden feedback

while (true)
{
    Console.WriteLine("Which device do you wish to send a message to? ");
    Console.Write("> ");
    string? deviceId = Console.ReadLine();

    await SendCloudToDeviceMessage(serviceClient, deviceId);
}

async Task SendCloudToDeviceMessage(ServiceClient serviceClient, string? deviceId)
{
    Console.WriteLine("What message payload do you want to send? ");
    Console.Write("> ");
    string? payload = Console.ReadLine();

    Message commandMessage = new Message(Encoding.ASCII.GetBytes(payload!));

    commandMessage.MessageId = Guid.NewGuid().ToString();
    commandMessage.Ack = DeliveryAcknowledgement.Full;
    commandMessage.ExpiryTimeUtc = DateTime.UtcNow.AddSeconds(10);

    await serviceClient.SendAsync(deviceId, commandMessage);
}


static async Task ReceiveFeedback(ServiceClient serviceClient)
{
    var feedbackReceiver = serviceClient.GetFeedbackReceiver();

    while (true)
    {
        var feedbackBatch = await feedbackReceiver.ReceiveAsync();

        if (feedbackBatch == null) continue;

        foreach (var record in feedbackBatch.Records)
        {
            var messageId = record.OriginalMessageId;
            var statusCode = record.StatusCode;

            Console.WriteLine($"Feedback for message '{messageId}', status code {statusCode}");
        }
        await feedbackReceiver.CompleteAsync(feedbackBatch);
    }
}