// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using pubsub.Interfaces;
// using pubsub.Services;

// namespace example_publish.Consumer
// {
//     public class Rabbit2 : Subscriber
//     {
//         private readonly IRabbitMQPersistentConnection _persistentConnection;
//         private readonly ILogger<Rabbit2> _logger;
//         private readonly string _exchangeName;
//         private readonly string _queueName;
//         private readonly string _routingKey;

//         public Rabbit2(IRabbitMQPersistentConnection persistentConnection,
//                        ILogger<Rabbit2> logger,
//                        string exchangeName,
//                        string queueName,
//                        string routingKey) : base(persistentConnection, logger, exchangeName, queueName, routingKey)
//         {
//             _persistentConnection = persistentConnection;
//             _logger = logger;
//             _exchangeName = exchangeName;
//             _queueName = queueName;
//             _routingKey = routingKey;
//         }

//         public override Task Process(string @event)
//         {
//             var teste = @event;

//             throw new System.NotImplementedException();
//         }
//     }
// }