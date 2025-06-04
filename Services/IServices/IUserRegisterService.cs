namespace picture_backend.Services.IServices
{
    public interface IUserRegisterService
    {
        Task<bool> RegisterAsync(string username, string password); // 返回注册是否成功
        Task<bool> CheckUsernameExistsAsync(string username); // 检查用户名是否已存在
    }
}
