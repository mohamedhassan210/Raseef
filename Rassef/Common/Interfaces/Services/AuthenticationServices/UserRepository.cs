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
                .Include(u => u.Groups)
                .FirstOrDefaultAsync(x => x.Id == user.Id);

            if (userEntity == null)
                return false;

            var group = await _context.UserGroups.FindAsync(groupId);
            if (group == null)
                return false;

            if (!userEntity.Groups.Any(g => g.Id == groupId))
            {
                userEntity.Groups.Add(group);
                await _context.SaveChangesAsync();
            }

            return true;
        }
        public async Task<bool> RemoveUserFromGroupAsync(User user, int groupId)
        {
            var userEntity = await _context.Users
                .Include(u => u.Groups)
                .FirstOrDefaultAsync(x => x.Id == user.Id);

            if (userEntity == null)
                return false;

            var groupToRemove = userEntity.Groups.FirstOrDefault(g => g.Id == groupId);

            if (groupToRemove == null)
                return false;

            userEntity.Groups.Remove(groupToRemove);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> ChangeUserGroupAsync(int userId, int newGroupId)
        {
            var userEntity = await _context.Users
                .Include(u => u.Groups)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (userEntity == null)
                return false;

            var newGroup = await _context.UserGroups.FindAsync(newGroupId);
            if (newGroup == null)
                return false;

            userEntity.Groups.Clear();

            userEntity.Groups.Add(newGroup);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
