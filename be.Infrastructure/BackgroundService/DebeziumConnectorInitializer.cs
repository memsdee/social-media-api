using System.Net.Http.Json;
using be.Infrastructure.Common.Appsetting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.BackgroundService;

public class DebeziumConnectorInitializer(
    IHttpClientFactory httpClientFactory,
    IOptions<DebeziumSettings> debeziumSettings,
    ILogger<DebeziumConnectorInitializer> logger) : IHostedService
{
    private readonly DebeziumSettings _debeziumValue = debeziumSettings.Value;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("debezium");

    public async Task StartAsync(CancellationToken ct)
    {
        var payload = new
        {
            name = _debeziumValue.Connector.Name,
            config = new Dictionary<string, object>
            {
                ["connector.class"] = _debeziumValue.Connector.ConnectorClass,
                ["database.hostname"] = _debeziumValue.Connector.DatabaseHostName,
                ["database.port"] = _debeziumValue.Connector.DatabasePort,
                ["database.user"] = _debeziumValue.Connector.DatabaseUser,
                ["database.password"] = _debeziumValue.Connector.DatabasePassword,
                ["database.dbname"] = _debeziumValue.Connector.DatabaseName,
                ["topic.prefix"] = _debeziumValue.Connector.DatabaseServerName,
                ["schema.include.list"] = _debeziumValue.Connector.SchemaIncludeList,
                ["table.include.list"] = _debeziumValue.Connector.TableIncludeList,
                ["plugin.name"] = _debeziumValue.Connector.PluginName,
                ["slot.name"] = _debeziumValue.Connector.SlotName,
                ["kafka.bootstrap.servers"] = _debeziumValue.Kafka.BootstrapServers,
                ["transforms"] = "outbox",
                ["transforms.outbox.type"] = "io.debezium.transforms.outbox.EventRouter",
                ["transforms.outbox.table.field.event.key"] = "id",
                ["transforms.outbox.table.field.event.type"] = "topic",
                ["transforms.outbox.table.field.event.payload"] = "payload"
            }
        };

        try
        {
            var check = await _httpClient.GetAsync($"/connectors/{_debeziumValue.Connector.Name}", ct);
            if (check.IsSuccessStatusCode)
            {
                logger.LogInformation("Debezium kết nối '{Name}' đã tồn tại", _debeziumValue.Connector.Name);
                return;
            }

            var response = await _httpClient.PostAsJsonAsync("/connectors", payload, ct);
            response.EnsureSuccessStatusCode();
            logger.LogInformation("Debezium đã đăng ký thành công connector '{Name}'", _debeziumValue.Connector.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Debezium đăng ký connector '{Name}' thất bại", _debeziumValue.Connector.Name);
            throw;
        }
    }

    public Task StopAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}