using picture_backend.Models;

namespace picture_backend.Repositories.IRepositories
{
    public interface IScriptAnalysisRepository
    {
        Task<ScriptAnalysis?> GetByScriptIdAsync(int scriptId);
        Task AddOrUpdateAsync(ScriptAnalysis analysis);
    }
}
