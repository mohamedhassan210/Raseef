namespace Rassef.Common.Interfaces
{
    public interface ITruckRepository : IRepository<Truck>
    {
        Task GetTruckStatus();


    }
}
