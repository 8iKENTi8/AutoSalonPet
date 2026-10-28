namespace AutoSalonPet.Services;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";

    public string SalesTopic { get; set; } = "sales";

    public string FeedGroupId { get; set; } = "autosalon-feed";

    public string FinanceGroupId { get; set; } = "autosalon-finance";
}
