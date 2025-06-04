using picture_backend.Models;

namespace picture_backend.Repositories.IRepositories
{
    public interface IScriptRepository
    {
        Task<Script> GetByIdAsync(int id);
        Task<IEnumerable<Script>> GetByUserIdAsync(int userId);
        Task<Script> AddAsync(Script script);
        Task<bool> UpdateAsync(int scriptId, string title, string content, int stage);
        Task DeleteAsync(int id);
    }
}
