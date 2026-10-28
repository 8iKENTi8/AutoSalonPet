using System.Text.Json;
using AutoSalonPet.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AutoSalonPet.Services;

public class SaleFeedConsumer : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly SalesFeedService _salesFeed;
    private readonly ILogger<SaleFeedConsumer> _logger;

    public SaleFeedConsumer(IOptions<KafkaSettings> settings, SalesFeedService salesFeed, ILogger<SaleFeedConsumer> logger)
    {
        _settings = settings.Value;
        _salesFeed = salesFeed;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private void ConsumeLoop(CancellationToken stoppingToken)
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

                try
                {
                    var sale = JsonSerializer.Deserialize<SaleRecord>(result.Message.Value);

                    if (sale is null)
                    {
                        _logger.LogWarning("Пустое сообщение пропущено (partition {Partition}, offset {Offset})", result.Partition.Value, result.Offset.Value);
                        continue;
                    }

                    _salesFeed.Add(sale);

                    _logger.LogInformation(
                        "[{Group}] Продажа из Kafka: {Brand} {Model} (partition {Partition}, offset {Offset})",
                        _settings.FeedGroupId,
                        sale.Car.Brand,
                        sale.Car.Model,
                        result.Partition.Value,
                        result.Offset.Value);
                }
                catch (JsonException)
                {
                    _logger.LogWarning("Некорректное сообщение пропущено (partition {Partition}, offset {Offset})", result.Partition.Value, result.Offset.Value);
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
