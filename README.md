Pub/Sub
=======

Publicar e Assinar mensagens através do RabbitMQ

## Referências
 - Foi utilizado o Polly para persistência da conexão.
 - Quando utilizado a Assinatura de fila, a estrutura de Exchange, fila, fila morta para controle de falhas são criados automaticamente

Objetivos:

1. Para trabalhar com o RabbitMQ no .NET é simples.

Para conectar ao RabbitMQ através de injeção de dependencia
```c#
    services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

        var factory = new ConnectionFactory()
        {
            HostName = Configuration["Rabbit:HostName"],
            DispatchConsumersAsync = true
        };

        factory.UserName = Configuration["Rabbit:UserName"];
        factory.Password = Configuration["Rabbit:Password"];

        return new DefaultRabbitMQPersistentConnection(factory, logger, int.Parse(Configuration["Rabbit:RetryCount"]));
    });
```

Configuração para publicar
```c#
  services.AddSingleton<IPublisher<WeatherForecast>>(sp => 
  {
      var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
      var logger = sp.GetRequiredService<ILogger<Publisher<WeatherForecast>>>();
      var exchangeName = Configuration["Rabbit:ExchangeName"];
      var routeKey = Configuration["Rabbit:RouteKey"];

      return new Publisher<WeatherForecast>(rabbitMQPersistentConnection, logger, exchangeName, routeKey);
  });
```

Configuração para Assinar
```c#
    services.AddHostedService<Rabbit1>(Span => {
        var rabbitMQPersistentConnection = Span.GetRequiredService<IRabbitMQPersistentConnection>();
        var logger = Span.GetRequiredService<ILogger<Rabbit1>>();
        var exchangeName = Configuration["Subscribe:ExchangeName"];
        var queueName = Configuration["Subscribe:QueueName"];
        var routeKey = Configuration["Subscribe:RouteKey"];

        return new Rabbit1(rabbitMQPersistentConnection, logger, exchangeName, queueName, routeKey);
    });
```

Publicando mensagens
```c#
  private readonly IPublisher<WeatherForecast> _publisher;
  ...
  _publisher.Publish(wf);
```

Assinando fila
```c#
  public class Rabbit1 : Subscriber
  {
      private readonly IRabbitMQPersistentConnection _persistentConnection;
      private readonly ILogger<Rabbit1> _logger;
      private readonly string _exchangeName;
      private readonly string _queueName;
      private readonly string _routingKey;

      public Rabbit1(IRabbitMQPersistentConnection persistentConnection,
                     ILogger<Rabbit1> logger,
                     string exchangeName,
                     string queueName,
                     string routingKey) : base(persistentConnection, logger, exchangeName, queueName, routingKey)
      {
          _persistentConnection = persistentConnection;
          _logger = logger;
          _exchangeName = exchangeName;
          _queueName = queueName;
          _routingKey = routingKey;
      }

      public override Task<(bool success, string message)> Process(string @event)
      {
          var teste = @event;

          throw new System.NotImplementedException();
      }
  }
```

Todas as dependencias para solução estão incluídas na solução.
Para executar os testes tenha o RabbitMQ server (veja http://www.rabbitmq.com/ para mais detalhes).
