namespace pubsub.Interfaces
{
    public interface IPublisher
    {
         void Publish(string message, string routeKey, string exchange);
    }
}