using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ServiceBusApp.Data;
using ServiceBusApp.Entities;

namespace ServiceBusApp
{
    public class ServiceBusFunction
    {
        private readonly ILogger<ServiceBusFunction> _logger;
        private readonly IUserRepository _userRepository;


        public ServiceBusFunction(ILogger<ServiceBusFunction> logger, IUserRepository userRepository = null)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        [Function(nameof(ServiceBusFunction))]
        public async Task Run([ServiceBusTrigger("myqueue1", Connection = "ServiceBusConnection", IsBatched = true)] ServiceBusReceivedMessage[] messages)
        {
            foreach (ServiceBusReceivedMessage message in messages)
            {
                try
                {
                    _logger.LogInformation("Message ID: {id}", message.MessageId);
                    _logger.LogInformation("Message Body: {body}", message.Body);
                    _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

                    var userData = JsonSerializer.Deserialize<UserData>(message.Body.ToString());
                    if (userData == null)
                    {
                        _logger.LogError("Unable to deserialize message body.");
                        return;
                    }

                    User user = await _userRepository.GetUserById(userData.Id.ToString());
                    if (user == null)
                    {
                        _logger.LogInformation("Creating new user in database.");
                        await _userRepository.CreateUser(userData);
                    }

                    var apiResponse = await SendUserDataToApi(userData);
                    _logger.LogInformation("API response: {response}", apiResponse);

                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing message body: {body}", message.Body);
                }
                catch (SqlException ex)
                {
                    _logger.LogError(ex, "Database error for message: {messageId}", message.MessageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {messageId}", message.MessageId);
                    throw;
                }
            }
        }

        private async Task<string> SendUserDataToApi(UserData userData)
        {
            // Имитация отправки данных в API
            _logger.LogInformation("Sending user data to API: {userData}", userData);
            return "Success";
        }

        public class UserData
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string Name { get; set; }
            public string Email { get; set; }
        }
    }
}