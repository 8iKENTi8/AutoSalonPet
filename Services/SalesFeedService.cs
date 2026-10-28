using AutoSalonPet.Models;

namespace AutoSalonPet.Services;

public class SalesFeedService
{
    private const int MaxRecords = 20;
    private readonly object gate = new();
    private readonly List<SaleRecord> newSales = [];
    private readonly List<SaleRecord> usedSales = [];

    public event Action? Changed;

    public IReadOnlyList<SaleRecord> GetNewSales()
    {
        lock (gate)
        {
            return newSales.ToList();
        }
    }

    public IReadOnlyList<SaleRecord> GetUsedSales()
    {
        lock (gate)
        {
            return usedSales.ToList();
        }
    }

    public void Add(SaleRecord sale)
    {
        lock (gate)
        {
            var sales = sale.Car.IsUsed ? usedSales : newSales;
            sales.Insert(0, sale);

            if (sales.Count > MaxRecords)
            {
                sales.RemoveAt(sales.Count - 1);
            }
        }

        Changed?.Invoke();
    }
}
