namespace be.Application.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(string topic, T message);
}