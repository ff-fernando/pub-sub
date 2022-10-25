using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using pubsub.Interfaces;

namespace example.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IPublisher _publisher;
        private readonly IConfiguration _configuration;

        public TestController(IPublisher publisher, 
            IConfiguration configuration)
        {
            _publisher = publisher;
            _configuration = configuration;
        }

        [HttpGet]
        public Test Get()
        {
            var obj = new Test { Message = "Hello World!" };
            var message = JsonSerializer.Serialize(obj);

            _publisher.Publish(message, _configuration["Rabbit:RouteKey"], _configuration["Rabbit:ExchangeName"]);

            return obj;
        }
    }
}