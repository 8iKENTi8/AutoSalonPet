using System.Text.Encodings.Web;
using System.Text.Json;
using AutoSalonPet.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AutoSalonPet.Services;

public class KafkaSaleProducer : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly KafkaSettings _settings;
    private readonly ILogger<KafkaSaleProducer> _logger;
    private readonly IProducer<string, string> _producer;

    public KafkaSaleProducer(IOptions<KafkaSettings> settings, ILogger<KafkaSaleProducer> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            MessageTimeoutMs = 5000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(SaleRecord sale, CancellationToken cancellationToken = default)
    {
        var message = new Message<string, string>
        {
            Key = sale.Car.Brand,
            Value = JsonSerializer.Serialize(sale, JsonOptions)
        };

        var result = await _producer.ProduceAsync(_settings.SalesTopic, message, cancellationToken);

        _logger.LogInformation(
            "Продажа {Brand} {Model} отправлена в Kafka (partition {Partition}, offset {Offset})",
            sale.Car.Brand,
            sale.Car.Model,
            result.Partition.Value,
            result.Offset.Value);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}
