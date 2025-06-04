using System.Threading.Tasks;
using picture_backend.Models;

namespace picture_backend.Services.IServices
{
    public interface IUserLoginService
    {
        Task<string?> LoginAsync(string username, string password); // 返回JWT令牌或null
    }
}