public class RequestStatusRepository : Repository<RequestStatuses>, IRequestStatusRepository
{
    public RequestStatusRepository(ApplicationDbContext context) : base(context)
    {
    }
}