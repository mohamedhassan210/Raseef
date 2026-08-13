public class PermitTypeRepository : Repository<PermitTypes>, IPermitTypeRepository
{
    public PermitTypeRepository(ApplicationDbContext context) : base(context)
    {
    }
}