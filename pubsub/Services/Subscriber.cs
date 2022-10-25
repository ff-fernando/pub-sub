using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pubsub.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace cartonizacao.Service
{
    public class Subscriber : IHostedService
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger _logger;
        private readonly IModel _channel;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly string _routingKey;
        private readonly bool _durable;
        private ushort _prefetchCount = 1;

        public Subscriber(
            IRabbitMQPersistentConnection persistentConnection,
            ILogger logger,
            string exchangeName,
            string queueName,
            string routingKey, 
            bool durable,
            bool createStructure = true,
            ushort prefetchCount = 1)
        {
            _persistentConnection = persistentConnection;
            _logger = logger;
            _exchangeName = exchangeName;
            _queueName = queueName;
            _routingKey = routingKey;
            _durable = durable;
            _prefetchCount = prefetchCount;

            _channel = _persistentConnection.CreateModel();
            
            if (createStructure)
                _channel = CreateConsumerChannel();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Subscribe();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.Dispose();
            return Task.CompletedTask;
        }

        public virtual async Task Process(string @event)
        {
            _logger.LogInformation($"Processing event: {@event}");
            await Task.CompletedTask;
        }

        public void Subscribe()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume");

            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            if (_channel != null)
            {
                _channel.QueueBind(queue: _queueName,
                                exchange: _exchangeName,
                                routingKey: _routingKey);

                _channel.BasicQos(prefetchSize: 0, prefetchCount: _prefetchCount, global: false);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = Encoding.UTF8.GetString(ea.Body.Span);
                    
                    try
                    {
                        await Process(body);

                        _channel.BasicAck(ea.DeliveryTag, false);

                        await Task.Yield();
                    }
                    catch (Exception ex)
                    {
                        Error(body, ex.Message, ex.StackTrace);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                };

                _channel.BasicConsume(queue: _queueName,
                                      autoAck: false, 
                                      consumer: consumer);
            }
            else
            {
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
            }
        }

        private void Error(string message, string errorMessage, string stackTrace)
        {
            try
            {
                IBasicProperties props = _channel.CreateBasicProperties();
                props.ContentType = "application/json";
                props.Type = "error";
                props.Headers = new Dictionary<string, object>();
                props.Headers.Add("error", $"{errorMessage} - {stackTrace}");

                var body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(exchange: $"dlx.{_exchangeName}",
                                        routingKey: _routingKey,
                                        basicProperties: props,
                                        body: body);   
            }
            catch (System.Exception ex)
            {
                _logger.LogCritical("Erro crítico ao enviar mensagem de erro para DLX", ex.Message);
            }
        }        

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _logger.LogTrace("Creating RabbitMQ DLX");

            _channel.ExchangeDeclare(exchange: $"dlx.{_exchangeName}",
                                    type: "fanout",
                                    durable: _durable);

            var args = new Dictionary<string, object>();
            args.Add("x-queue-mode","lazy");
            args.Add("x-queue-type", "classic");
            _channel.QueueDeclare(queue: $"dlq.{_queueName}",
                                durable: _durable,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            _channel.QueueBind(queue: $"dlq.{_queueName}",
                                exchange: $"dlx.{_exchangeName}",
                                routingKey: _routingKey);

            _logger.LogTrace("Creating RabbitMQ consumer channel");

            _channel.ExchangeDeclare(exchange: _exchangeName,
                                    type: "direct");

            var argsx = new Dictionary<string, object>();
            argsx.Add("x-dead-letter-exchange", $"dlx.{_exchangeName}");
            argsx.Add("x-queue-mode","lazy");
            _channel.QueueDeclare(queue: _queueName,
                                 durable: _durable,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            _channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

                Subscribe();
            };

            return _channel;
        }
    }
}