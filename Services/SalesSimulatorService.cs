using AutoSalonPet.Models;
using Confluent.Kafka;

namespace AutoSalonPet.Services;

public class SalesSimulatorService : BackgroundService
{
    private readonly KafkaSaleProducer _producer;
    private readonly ILogger<SalesSimulatorService> _logger;
    private readonly Random _random = new();
    private int _nextId = 1000;

    public SalesSimulatorService(KafkaSaleProducer producer, ILogger<SalesSimulatorService> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await AddRandomSalesAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await AddRandomSalesAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task AddRandomSalesAsync(CancellationToken cancellationToken)
    {
        await PublishSaleAsync(CreateRandomCar(isUsed: false), cancellationToken);
        await PublishSaleAsync(CreateRandomCar(isUsed: true), cancellationToken);
    }

    private async Task PublishSaleAsync(Car car, CancellationToken cancellationToken)
    {
        var sale = new SaleRecord { Car = car, SoldAt = DateTime.Now };

        try
        {
            await _producer.PublishAsync(sale, cancellationToken);
        }
        catch (KafkaException ex)
        {
            _logger.LogWarning("Не удалось отправить продажу в Kafka: {Message}", ex.Message);
        }
    }

    private Car CreateRandomCar(bool isUsed)
    {
        var model = carModels[_random.Next(carModels.Length)];
        var currentYear = DateTime.Now.Year;
        var year = isUsed
            ? _random.Next(2018, currentYear - 1)
            : _random.Next(currentYear - 1, currentYear + 1);
        var age = currentYear - year;

        var price = model.BasePrice
            * (decimal)Math.Pow(0.88, age)
            * (0.93m + (decimal)_random.NextDouble() * 0.14m);
        price = Math.Round(price / 10_000m) * 10_000m;

        return new Car
        {
            Id = _nextId++,
            Brand = model.Brand,
            Model = model.Model,
            Year = year,
            HorsePower = _random.Next(model.MinHorsePower, model.MaxHorsePower + 1),
            IsUsed = isUsed,
            MileageKm = isUsed ? _random.Next(age * 8_000, age * 22_000 + 1) : 0,
            Color = carColors[_random.Next(carColors.Length)],
            Price = price
        };
    }

    private sealed record CarModel(string Brand, string Model, decimal BasePrice, int MinHorsePower, int MaxHorsePower);

    private static readonly CarModel[] carModels =
    [
        new("Audi", "A3", 3_500_000, 110, 190),
        new("Audi", "A4", 4_200_000, 150, 265),
        new("Audi", "A6", 6_500_000, 190, 340),
        new("Audi", "Q5", 5_500_000, 150, 265),
        new("Audi", "Q7", 8_000_000, 250, 340),
        new("Audi", "Q8", 11_000_000, 300, 507),
        new("Audi", "e-tron", 7_000_000, 230, 408),
        new("Volkswagen", "Polo", 1_500_000, 90, 125),
        new("Volkswagen", "Golf", 2_400_000, 110, 300),
        new("Volkswagen", "Passat", 3_000_000, 150, 280),
        new("Volkswagen", "Tiguan", 3_700_000, 150, 245),
        new("Volkswagen", "Touareg", 6_800_000, 250, 340),
        new("Volkswagen", "Arteon", 4_000_000, 190, 320),
        new("Volkswagen", "Teramont", 4_800_000, 220, 280),
        new("Skoda", "Fabia", 1_200_000, 90, 125),
        new("Skoda", "Rapid", 1_300_000, 90, 125),
        new("Skoda", "Octavia", 2_600_000, 110, 245),
        new("Skoda", "Superb", 3_400_000, 150, 280),
        new("Skoda", "Karoq", 2_700_000, 110, 190),
        new("Skoda", "Kodiaq", 3_600_000, 150, 220)
    ];

    private static readonly string[] carColors =
    [
        "Белый", "Чёрный", "Серый", "Синий", "Красный", "Серебристый"
    ];
}
