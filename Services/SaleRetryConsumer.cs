using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AutoSalonPet.Services;

public class SaleRetryConsumer : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly DeadLetterPublisher _deadLetters;
    private readonly ILogger<SaleRetryConsumer> _logger;

    public SaleRetryConsumer(IOptions<KafkaSettings> settings, DeadLetterPublisher deadLetters, ILogger<SaleRetryConsumer> logger)
    {
        _settings = settings.Value;
        _deadLetters = deadLetters;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => ConsumeLoopAsync(stoppingToken), stoppingToken);
    }

    private async Task ConsumeLoopAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.RetryGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_settings.RetryTopic);

        _logger.LogInformation("Retry-консюмер запущен, группа '{Group}', топик '{Topic}'...", _settings.RetryGroupId, _settings.RetryTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                var retryCount = KafkaHeaders.GetRetryCount(result.Message.Headers);

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                await _deadLetters.RepublishToSalesAsync(result, retryCount, stoppingToken);

                _logger.LogInformation(
                    "[{Group}] Повторная отправка в sales, попытка {RetryCount} (ключ {Key})",
                    _settings.RetryGroupId,
                    retryCount,
                    result.Message.Key);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            consumer.Close();
        }
    }
}
