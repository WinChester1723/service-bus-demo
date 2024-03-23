using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;


namespace ServiceBusApp
{
    public class ServiceBusFunction
    {
        private readonly ILogger<ServiceBusFunction> _logger;


        public ServiceBusFunction(ILogger<ServiceBusFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ServiceBusFunction))]
        public async Task Run([ServiceBusTrigger("myqueue1", Connection = "ServiceBusConnection", IsBatched = true)] ServiceBusReceivedMessage[] messages)
        {
            foreach (ServiceBusReceivedMessage message in messages)
            {

                _logger.LogInformation("Message ID: {id}", message.MessageId);
                _logger.LogInformation("Message Body: {body}", message.Body);
                _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            }
        }
    }
}