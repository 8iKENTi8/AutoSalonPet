using System.Text.Json;
using AutoSalonPet.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AutoSalonPet.Services;

public class SaleFeedConsumer : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly SalesFeedService _salesFeed;
    private readonly DeadLetterPublisher _deadLetters;
    private readonly ILogger<SaleFeedConsumer> _logger;

    public SaleFeedConsumer(
        IOptions<KafkaSettings> settings,
        SalesFeedService salesFeed,
        DeadLetterPublisher deadLetters,
        ILogger<SaleFeedConsumer> logger)
    {
        _settings = settings.Value;
        _salesFeed = salesFeed;
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
            GroupId = _settings.FeedGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_settings.SalesTopic);

        _logger.LogInformation("Консюмер ленты запущен, группа '{Group}'...", _settings.FeedGroupId);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                var retryCount = KafkaHeaders.GetRetryCount(result.Message.Headers);

                SaleRecord? sale;
                try
                {
                    sale = JsonSerializer.Deserialize<SaleRecord>(result.Message.Value);
                }
                catch (JsonException)
                {
                    _logger.LogWarning(
                        "Битое сообщение отправлено в DLQ (partition {Partition}, offset {Offset})",
                        result.Partition.Value,
                        result.Offset.Value);

                    await _deadLetters.PublishToDlqAsync(result, retryCount, "Некорректный JSON", stoppingToken);
                    continue;
                }

                if (sale is null)
                {
                    _logger.LogWarning(
                        "Пустое сообщение отправлено в DLQ (partition {Partition}, offset {Offset})",
                        result.Partition.Value,
                        result.Offset.Value);

                    await _deadLetters.PublishToDlqAsync(result, retryCount, "Пустое сообщение", stoppingToken);
                    continue;
                }

                if (retryCount >= _settings.MaxRetries)
                {
                    _logger.LogWarning(
                        "Исчерпаны попытки ({RetryCount}), сообщение отправлено в DLQ: {Brand} {Model}",
                        retryCount,
                        sale.Car.Brand,
                        sale.Car.Model);

                    await _deadLetters.PublishToDlqAsync(result, retryCount, "Исчерпаны попытки", stoppingToken);
                    continue;
                }

                try
                {
                    if (sale.Car.Brand == "FAIL")
                    {
                        throw new InvalidOperationException("Тестовая ошибка обработки");
                    }

                    _salesFeed.Add(sale);

                    _logger.LogInformation(
                        "[{Group}] Продажа из Kafka: {Brand} {Model} (partition {Partition}, offset {Offset}, попытка {Attempt})",
                        _settings.FeedGroupId,
                        sale.Car.Brand,
                        sale.Car.Model,
                        result.Partition.Value,
                        result.Offset.Value,
                        retryCount + 1);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        "Ошибка обработки, отправка в retry (попытка {Attempt} из {Max}): {Message}",
                        retryCount + 1,
                        _settings.MaxRetries,
                        ex.Message);

                    await _deadLetters.PublishToRetryAsync(result, retryCount + 1, ex.Message, stoppingToken);
                }
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
