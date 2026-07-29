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
            var group = await _context.UserGroups
                .FirstOrDefaultAsync(x => x.Id == groupId);
            if (group == null)
                return false;
            userEntity.GroupId = groupId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUserFromGroupAsync(User user, int groupId)
        {
            var userEntity = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == user.Id);
            if (userEntity == null)
                return false;
            if (userEntity.GroupId != groupId)
                return false;
            userEntity.GroupId =default;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeUserGroupAsync(int userId, int newGroupId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return false;
            var group = await _context.UserGroups
                .FirstOrDefaultAsync(x => x.Id == newGroupId);
            if (group == null)
                return false;
            user.GroupId = newGroupId;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
