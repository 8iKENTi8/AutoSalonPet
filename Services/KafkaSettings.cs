namespace AutoSalonPet.Services;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";

    public string SalesTopic { get; set; } = "sales";

    public string RetryTopic { get; set; } = "sales.retry";

    public string DlqTopic { get; set; } = "sales.dlq";

    public string FeedGroupId { get; set; } = "autosalon-feed";

    public string RetryGroupId { get; set; } = "autosalon-feed-retry";

    public string FinanceGroupId { get; set; } = "autosalon-finance";

    public int MaxRetries { get; set; } = 3;
}
