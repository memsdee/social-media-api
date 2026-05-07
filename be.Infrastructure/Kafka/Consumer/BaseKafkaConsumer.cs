using System.Text;
using System.Text.Json;
using be.Infrastructure.Common.Appsetting;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Kafka.Consumer;

public abstract class BaseKafkaConsumer<T>(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger logger) : Microsoft.Extensions.Hosting.BackgroundService
{
    private const int MaxRetries = 3;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected abstract string Topic { get; }
    protected abstract Task HandleAsync(T payload, CancellationToken stoppingToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaOptions.Value.BootstrapServers,
            GroupId = Topic,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        if (kafkaOptions.Value.UseSaslSsl)
        {
            config.SaslUsername = kafkaOptions.Value.UserName;
            config.SaslPassword = kafkaOptions.Value.Password;
            config.SecurityProtocol = SecurityProtocol.SaslSsl;
            config.SaslMechanism = SaslMechanism.Plain;
        }

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? result = null;
            try
            {
                result = consumer.Consume(stoppingToken);

                var payload = JsonSerializer.Deserialize<T>(result.Message.Value, JsonOptions)
                              ?? throw new InvalidOperationException($"Deserialize thất bại: {result.Message.Value}");

                await HandleWithRetryAsync(payload, stoppingToken);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                logger.LogWarning("[{Topic}] Topic chưa tồn tại, chờ...", Topic);
                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[{Topic}] Đẩy message vào DLT | Payload: {Payload}",
                    Topic, result?.Message.Value);

                if (result is not null)
                    await SendToDltAsync(result, ex, stoppingToken);

                if (result is not null)
                    consumer.Commit(result);
            }
        }

        consumer.Close();
    }

    private async Task HandleWithRetryAsync(T payload, CancellationToken ct)
    {
        var attempt = 0;
        while (true)
            try
            {
                attempt++;
                await HandleAsync(payload, ct);
                return;
            }
            catch (Exception ex) when (attempt < MaxRetries)
            {
                logger.LogWarning(ex, "[{Topic}] Retry {Attempt}/{Max}", Topic, attempt, MaxRetries);
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct);
            }
    }

    private async Task SendToDltAsync(ConsumeResult<string, string> result, Exception ex, CancellationToken ct)
    {
        var dltMessage = new Message<string, string>
        {
            Key = result.Message.Key,
            Value = result.Message.Value,
            Headers = new Headers
            {
                { "error-message", Encoding.UTF8.GetBytes(ex.Message) },
                { "original-topic", Encoding.UTF8.GetBytes(Topic) },
                { "failed-at", Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString("O")) }
            }
        };

        await dltProducer.ProduceAsync($"{Topic}.DLT", dltMessage, ct);
    }
}