namespace AutoSalonPet.Models;

public class Car
{
    public int Id { get; set; }

    public string Brand { get; set; } = "";

    public string Model { get; set; } = "";

    public int Year { get; set; }

    public int HorsePower { get; set; }

    public bool IsUsed { get; set; }

    public int MileageKm { get; set; }

    public string Color { get; set; } = "";

    public decimal Price { get; set; }
}
