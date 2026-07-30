namespace Rassef.Common.Interfaces.Services.AuthenticationServices
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        public async Task<bool> AddUserToGroupAsync(User user, int groupId)
        {
            var userEntity = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == user.Id);

            if (userEntity == null)
                return false;

            var group = await _context.UserGroups.FindAsync(groupId);

            if (group == null)
                return false;

            userEntity.GroupId = groupId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeUserGroupAsync(int userId, int newGroupId)
        {
            var userEntity = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (userEntity == null)
                return false;

            var newGroup = await _context.UserGroups.FindAsync(newGroupId);

            if (newGroup == null)
                return false;

            userEntity.GroupId = newGroupId;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
