using picture_backend.Models;
using picture_backend.Repositories.IRepositories;
using picture_backend.Services.IServices;
using picture_backend.Utils;

namespace picture_backend.Services
{
    public class UserRegisterService : IUserRegisterService
    {
        private readonly IUserRepository _userRepository;
        public UserRegisterService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<bool> RegisterAsync(string username, string password)
        {
            // 检查用户名是否已存在
            if (await CheckUsernameExistsAsync(username))
            {
                return false; // 用户名已存在，注册失败
            }
            // 创建新用户
            var newUser = new User
            {
                Username = username,
                PasswordHash = PasswordHelper.HashPassword(password) // 使用密码哈希函数
            };
            return await _userRepository.AddUserAsync(newUser);
        }
        public async Task<bool> CheckUsernameExistsAsync(string username)
        {
            var existingUser = await _userRepository.GetUserByUsernameAsync(username);
            return existingUser != null; // 如果找到了用户，则用户名已存在
        }
    }
}
