using picture_backend.Models;

namespace picture_backend.Services.IServices
{
    public interface IScriptAnalysisService
    {
        Task<ScriptAnalysis> AnalyzeScriptAsync(int scriptId, string scriptContent);
        Task<ScriptAnalysis> GetByScriptIdAsync(int scriptId);
    }
}
