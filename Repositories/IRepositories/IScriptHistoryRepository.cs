using System.Collections.Generic;
using System.Threading.Tasks;
using picture_backend.Models;

namespace picture_backend.Repositories.IRepositories
{
    public interface IScriptHistoryRepository
    {
        Task AddAsync(ScriptHistory history);
        Task<List<ScriptHistory>> GetByScriptIdAsync(int scriptId);
    }

}
