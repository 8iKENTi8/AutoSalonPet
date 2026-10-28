namespace AutoSalonPet.Services;

public class FinanceService
{
    private readonly object gate = new();
    private decimal revenue;
    private int soldCars;

    public event Action? Changed;

    public decimal InitialBalance { get; } = 20_000_000m;

    public decimal Revenue
    {
        get
        {
            lock (gate)
            {
                return revenue;
            }
        }
    }

    public int SoldCars
    {
        get
        {
            lock (gate)
            {
                return soldCars;
            }
        }
    }

    public decimal Balance => InitialBalance + Revenue;

    public void RegisterSale(decimal amount)
    {
        lock (gate)
        {
            revenue += amount;
            soldCars++;
        }

        Changed?.Invoke();
    }
}
