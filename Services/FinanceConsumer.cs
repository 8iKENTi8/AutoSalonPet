using System.Text.Json;
using AutoSalonPet.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AutoSalonPet.Services;

public class FinanceConsumer : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly FinanceService _finance;
    private readonly ILogger<FinanceConsumer> _logger;

    public FinanceConsumer(IOptions<KafkaSettings> settings, FinanceService finance, ILogger<FinanceConsumer> logger)
    {
        _settings = settings.Value;
        _finance = finance;
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
            GroupId = _settings.FinanceGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_settings.SalesTopic);

        _logger.LogInformation("Финансовый консюмер запущен, группа '{Group}'...", _settings.FinanceGroupId);

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

                    _finance.RegisterSale(sale.Car.Price);

                    _logger.LogInformation(
                        "[{Group}] Начислено {Amount:N0} ₽ за {Brand} {Model} (partition {Partition}, offset {Offset})",
                        _settings.FinanceGroupId,
                        sale.Car.Price,
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
