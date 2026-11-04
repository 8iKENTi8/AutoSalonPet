using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AutoSalonPet.Services;

public class DeadLetterPublisher : IDisposable
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<DeadLetterPublisher> _logger;
    private readonly IProducer<string, string> _producer;

    public DeadLetterPublisher(IOptions<KafkaSettings> settings, ILogger<DeadLetterPublisher> logger)
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

    public Task PublishToRetryAsync(ConsumeResult<string, string> result, int retryCount, string error, CancellationToken cancellationToken)
    {
        return PublishAsync(
            _settings.RetryTopic,
            result.Message.Key,
            result.Message.Value,
            KafkaHeaders.CreateHeaders(retryCount, error),
            cancellationToken);
    }

    public Task PublishToDlqAsync(ConsumeResult<string, string> result, int retryCount, string error, CancellationToken cancellationToken)
    {
        return PublishAsync(
            _settings.DlqTopic,
            result.Message.Key,
            result.Message.Value,
            KafkaHeaders.CreateHeaders(retryCount, error),
            cancellationToken);
    }

    public Task RepublishToSalesAsync(ConsumeResult<string, string> result, int retryCount, CancellationToken cancellationToken)
    {
        return PublishAsync(
            _settings.SalesTopic,
            result.Message.Key,
            result.Message.Value,
            KafkaHeaders.CreateHeaders(retryCount),
            cancellationToken);
    }

    private async Task PublishAsync(string topic, string? key, string value, Headers headers, CancellationToken cancellationToken)
    {
        var message = new Message<string, string>
        {
            Key = key!,
            Value = value,
            Headers = headers
        };

        await _producer.ProduceAsync(topic, message, cancellationToken);

        _logger.LogInformation("Сообщение отправлено в '{Topic}' (ключ {Key})", topic, key);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}
