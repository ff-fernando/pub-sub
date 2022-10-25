using System;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using pubsub.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace pubsub.Services
{
    public class Publisher : IPublisher, IDisposable
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<Publisher> _logger;
        private readonly int _retryCount;
        private readonly IModel _channel;
        public bool CreateStructure { get; set; }

        public Publisher(IRabbitMQPersistentConnection persistentConnection,
                         ILogger<Publisher> logger,
                         int retryCount = 5)
        {
            _persistentConnection = persistentConnection;
            _logger = logger;
            _retryCount = retryCount;

            _channel = _persistentConnection.CreateModel();
        }

        public void Publish(string message, string routeKey, string exchange)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", message.ToString(), $"{time.TotalSeconds:n1}", ex.Message);
                });

            var eventName = message.GetType().Name;

            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", message.ToString(), eventName);
                           
            var body = Encoding.UTF8.GetBytes(message);
            
            policy.Execute(() =>
            {
                var properties = _channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", message.ToString());

                _channel.BasicPublish(
                    exchange: exchange,
                    routingKey: routeKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            });
        }

        public void Dispose()
        {
            if (_channel != null)
            {
                _channel.Dispose();
            }
        }
    }
}