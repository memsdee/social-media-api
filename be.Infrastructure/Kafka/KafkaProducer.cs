using System.Text.Json;
using be.Application.Interfaces;
using Confluent.Kafka;

namespace be.Infrastructure.Kafka;

public class KafkaProducer(IProducer<string, string> producer) : IEventBus
{
    public async Task PublishAsync<T>(string topic, T message)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        await producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = jsonMessage
        });
    }
}