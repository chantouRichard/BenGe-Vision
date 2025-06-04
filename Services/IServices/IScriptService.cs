using picture_backend.Models;
using picture_backend.DTO;
using picture_backend.Entity;

namespace picture_backend.Services.IServices
{
    public interface IScriptService
    {
        Task<ScriptDetailDto?> GetScriptByIdAsync(int id);
        Task<IEnumerable<Script>> GetUserScriptsAsync(int userId);
        Task<Script> CreateScriptAsync(Script script);
        Task UpdateScriptAsync(int scriptId, string title, string content, int stage);
        Task DeleteScriptAsync(int id);
        Task<ScriptDetailDto> InitializeScriptAsync(int userId);
        // 模拟调用AI,返回AI的分析结果
        Task<ScriptFrameworkDto?> GenFrame(ScriptReplyRequestEntity request, List<ScriptHistory> History, string scriptcontent);
        IAsyncEnumerable<string> GenFrameStreamAsync(ScriptReplyRequestEntity request);

        Task<ScriptAnalysis?> AnalyzeScriptContent(string scriptContent, int scriptId);

        Task<ScriptDetailDto?> GetCompSctiptAndDesc(Script script);
        Task<string> VisualizeScriptAsync(int scriptId, int elementId);
    }
}
