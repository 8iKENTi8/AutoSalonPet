using AutoSalonPet.Models;

namespace AutoSalonPet.Services;

public class CarCatalogService
{
    private readonly List<Car> cars =
    [
        new()
        {
            Id = 1,
            Brand = "Audi",
            Model = "A3",
            Year = 2022,
            HorsePower = 150,
            IsUsed = false,
            MileageKm = 0,
            Color = "Белый",
            Price = 3_200_000
        },
        new()
        {
            Id = 2,
            Brand = "Audi",
            Model = "A4",
            Year = 2021,
            HorsePower = 190,
            IsUsed = true,
            MileageKm = 48_000,
            Color = "Чёрный",
            Price = 3_900_000
        },
        new()
        {
            Id = 3,
            Brand = "Audi",
            Model = "A6",
            Year = 2022,
            HorsePower = 245,
            IsUsed = false,
            MileageKm = 0,
            Color = "Серый",
            Price = 6_200_000
        },
        new()
        {
            Id = 4,
            Brand = "Audi",
            Model = "Q5",
            Year = 2021,
            HorsePower = 249,
            IsUsed = true,
            MileageKm = 62_000,
            Color = "Синий",
            Price = 5_400_000
        },
        new()
        {
            Id = 5,
            Brand = "Audi",
            Model = "Q7",
            Year = 2020,
            HorsePower = 340,
            IsUsed = true,
            MileageKm = 85_000,
            Color = "Чёрный",
            Price = 7_800_000
        },
        new()
        {
            Id = 6,
            Brand = "Audi",
            Model = "Q8",
            Year = 2023,
            HorsePower = 340,
            IsUsed = false,
            MileageKm = 0,
            Color = "Белый",
            Price = 11_500_000
        },
        new()
        {
            Id = 7,
            Brand = "Audi",
            Model = "e-tron",
            Year = 2022,
            HorsePower = 313,
            IsUsed = true,
            MileageKm = 30_000,
            Color = "Серебристый",
            Price = 6_900_000
        },
        new()
        {
            Id = 8,
            Brand = "Volkswagen",
            Model = "Polo",
            Year = 2021,
            HorsePower = 110,
            IsUsed = true,
            MileageKm = 54_000,
            Color = "Белый",
            Price = 1_450_000
        },
        new()
        {
            Id = 9,
            Brand = "Volkswagen",
            Model = "Golf",
            Year = 2021,
            HorsePower = 150,
            IsUsed = true,
            MileageKm = 41_000,
            Color = "Серый",
            Price = 2_300_000
        },
        new()
        {
            Id = 10,
            Brand = "Volkswagen",
            Model = "Passat",
            Year = 2020,
            HorsePower = 190,
            IsUsed = true,
            MileageKm = 70_000,
            Color = "Чёрный",
            Price = 2_800_000
        },
        new()
        {
            Id = 11,
            Brand = "Volkswagen",
            Model = "Tiguan",
            Year = 2022,
            HorsePower = 180,
            IsUsed = false,
            MileageKm = 0,
            Color = "Синий",
            Price = 3_600_000
        },
        new()
        {
            Id = 12,
            Brand = "Volkswagen",
            Model = "Touareg",
            Year = 2021,
            HorsePower = 340,
            IsUsed = true,
            MileageKm = 58_000,
            Color = "Чёрный",
            Price = 6_700_000
        },
        new()
        {
            Id = 13,
            Brand = "Volkswagen",
            Model = "Arteon",
            Year = 2021,
            HorsePower = 280,
            IsUsed = true,
            MileageKm = 39_000,
            Color = "Красный",
            Price = 3_900_000
        },
        new()
        {
            Id = 14,
            Brand = "Volkswagen",
            Model = "Teramont",
            Year = 2022,
            HorsePower = 249,
            IsUsed = false,
            MileageKm = 0,
            Color = "Серебристый",
            Price = 4_700_000
        },
        new()
        {
            Id = 15,
            Brand = "Skoda",
            Model = "Fabia",
            Year = 2020,
            HorsePower = 110,
            IsUsed = true,
            MileageKm = 66_000,
            Color = "Белый",
            Price = 1_200_000
        },
        new()
        {
            Id = 16,
            Brand = "Skoda",
            Model = "Rapid",
            Year = 2021,
            HorsePower = 125,
            IsUsed = true,
            MileageKm = 59_000,
            Color = "Серый",
            Price = 1_350_000
        },
        new()
        {
            Id = 17,
            Brand = "Skoda",
            Model = "Octavia",
            Year = 2022,
            HorsePower = 150,
            IsUsed = false,
            MileageKm = 0,
            Color = "Синий",
            Price = 2_600_000
        },
        new()
        {
            Id = 18,
            Brand = "Skoda",
            Model = "Superb",
            Year = 2021,
            HorsePower = 190,
            IsUsed = true,
            MileageKm = 47_000,
            Color = "Чёрный",
            Price = 3_300_000
        },
        new()
        {
            Id = 19,
            Brand = "Skoda",
            Model = "Karoq",
            Year = 2021,
            HorsePower = 150,
            IsUsed = true,
            MileageKm = 38_000,
            Color = "Белый",
            Price = 2_700_000
        },
        new()
        {
            Id = 20,
            Brand = "Skoda",
            Model = "Kodiaq",
            Year = 2022,
            HorsePower = 180,
            IsUsed = false,
            MileageKm = 0,
            Color = "Серебристый",
            Price = 3_800_000
        }
    ];

    public IReadOnlyList<Car> Cars => cars;

    public decimal StockValue => cars.Sum(car => car.Price);
}
