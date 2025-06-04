using Microsoft.EntityFrameworkCore;
using picture_backend.Models;
using picture_backend.Repositories.IRepositories;

namespace picture_backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Models.AppDbContext _context;
        // 构造函数实现依赖注入
        public UserRepository(Models.AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
        // 可能找不到用户，所以返回值为 User?，表示可能为 null
        public async Task<User?> GetUserByIdAsync(int Id)
        {
            return await _context.Users.FindAsync(Id);
        }
        public async Task<User?> GetUserByUsernameAsync(string Username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == Username);
        }

        public async Task<bool> AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
            {
                return false;
            }
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
