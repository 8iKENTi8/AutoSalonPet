namespace AutoSalonPet.Models;

public class SaleRecord
{
    public required Car Car { get; init; }

    public required DateTime SoldAt { get; init; }
}
