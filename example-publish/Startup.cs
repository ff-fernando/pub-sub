using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using pubsub.Interfaces;
using pubsub.Services;
using RabbitMQ.Client;

namespace example_pubsub
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddControllers();
            // services.AddSwaggerGen(c =>
            // {
            //     c.SwaggerDoc("v1", new OpenApiInfo { Title = "example_pubsub", Version = "v1" });
            // });
            
            // services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            // {
            //     var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

            //     var factory = new ConnectionFactory()
            //     {
            //         HostName = Configuration["Rabbit:HostName"],
            //         DispatchConsumersAsync = true
            //     };

            //     factory.UserName = Configuration["Rabbit:UserName"];
            //     factory.Password = Configuration["Rabbit:Password"];

            //     return new DefaultRabbitMQPersistentConnection(factory, logger, int.Parse(Configuration["Rabbit:RetryCount"]));
            // });

            // services.AddSingleton<IPublisher>(sp => 
            // {
            //     var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
            //     var logger = sp.GetRequiredService<ILogger<Publisher>>();
            //     var exchangeName = Configuration["Rabbit:ExchangeName"];
            //     var routeKey = Configuration["Rabbit:RouteKey"];

            //     return new Publisher(rabbitMQPersistentConnection, logger);
            // });

            // services.AddSingleton<IPublisher>(sp => 
            // {
            //     var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
            //     var logger = sp.GetRequiredService<ILogger<Publisher>>();
            //     var exchangeName = Configuration["Rabbit2:ExchangeName"];
            //     var routeKey = Configuration["Rabbit2:RouteKey"];

            //     return new Publisher(rabbitMQPersistentConnection, logger);
            // });

            // services.AddHostedService<Rabbit1>(Span => {
            //     var rabbitMQPersistentConnection = Span.GetRequiredService<IRabbitMQPersistentConnection>();
            //     var logger = Span.GetRequiredService<ILogger<Rabbit1>>();
            //     var exchangeName = Configuration["Subscribe:ExchangeName"];
            //     var queueName = Configuration["Subscribe:QueueName"];
            //     var routeKey = Configuration["Subscribe:RouteKey"];

            //     return new Rabbit1(rabbitMQPersistentConnection, logger, exchangeName, queueName, routeKey);
            // });

            // services.AddHostedService<Rabbit2>(Span => {
            //     var rabbitMQPersistentConnection = Span.GetRequiredService<IRabbitMQPersistentConnection>();
            //     var logger = Span.GetRequiredService<ILogger<Rabbit2>>();
            //     var exchangeName = Configuration["Subscribe2:ExchangeName"];
            //     var queueName = Configuration["Subscribe2:QueueName"];
            //     var routeKey = Configuration["Subscribe2:RouteKey"];

            //     return new Rabbit2(rabbitMQPersistentConnection, logger, exchangeName, queueName, routeKey);
            // });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "example_pubsub v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
