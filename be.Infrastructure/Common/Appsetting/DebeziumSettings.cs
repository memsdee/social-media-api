using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class DebeziumSettings
{
    [Required(ErrorMessage = "Debezium Kafka Connect Url đang trống")]
    public string KafkaConnectUrl { get; set; } = null!;

    [Required(ErrorMessage = "Debezium Connector đang trống")]
    public DebeziumConnectorSettings Connector { get; set; } = new();

    [Required(ErrorMessage = "Debezium Kafka đang trống")]
    public DebeziumKafkaSettings Kafka { get; set; } = new();
}

public class DebeziumConnectorSettings
{
    [Required(ErrorMessage = "Debezium Connector Name đang trống")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Debezium ConnectorClass đang trống")]
    public string ConnectorClass { get; set; } = null!;

    [Required(ErrorMessage = "Debezium DatabaseHostname đang trống")]
    public string DatabaseHostName { get; set; } = null!;

    [Range(1, 65535, ErrorMessage = "Debezium DatabasePort không hợp lệ")]
    public int DatabasePort { get; set; }

    [Required(ErrorMessage = "Debezium DatabaseUser đang trống")]
    public string DatabaseUser { get; set; } = null!;

    [Required(ErrorMessage = "Debezium DatabasePassword đang trống")]
    public string DatabasePassword { get; set; } = null!;

    [Required(ErrorMessage = "Debezium DatabaseName đang trống")]
    public string DatabaseName { get; set; } = null!;

    [Required(ErrorMessage = "Debezium DatabaseServerName đang trống")]
    public string DatabaseServerName { get; set; } = null!;

    [Required(ErrorMessage = "Debezium SchemaIncludeList đang trống")]
    public string SchemaIncludeList { get; set; } = null!;

    [Required(ErrorMessage = "Debezium TableIncludeList đang trống")]
    public string TableIncludeList { get; set; } = null!;

    [Required(ErrorMessage = "Debezium PluginName đang trống")]
    public string PluginName { get; set; } = null!;

    [Required(ErrorMessage = "Debezium SlotName đang trống")]
    public string SlotName { get; set; } = null!;
}

public class DebeziumKafkaSettings
{
    [Required(ErrorMessage = "Debezium Kafka BootstrapServers đang trống")]
    public string BootstrapServers { get; set; } = null!;
}