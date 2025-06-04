using picture_backend.Models;
using picture_backend.Repositories.IRepositories;
using picture_backend.Services.IServices;

namespace picture_backend.Services
{
    public class ScriptAnalysisService : IScriptAnalysisService
    {
        private readonly IScriptAnalysisRepository _repository;
        private readonly IScriptRepository _scriptRepository;
        private readonly IAIService _aiService;

        public ScriptAnalysisService(IScriptAnalysisRepository repository, IScriptRepository scriptRepo, IAIService aIService)
        {
            _repository = repository;
            _scriptRepository = scriptRepo;
            _aiService = aIService;
        }

        public async Task<ScriptAnalysis> AnalyzeScriptAsync(int scriptId, string scriptContent)
        {
            var script = await _scriptRepository.GetByIdAsync(scriptId);
            if(script == null) throw new Exception("Script not found.");

            var existing = await _repository.GetByScriptIdAsync(scriptId);
            if (existing != null && existing.AnalyzedAt >= script.LastUpdated)
            {
                return existing;
            }

            var result = await _aiService.AnalyzeScriptContent(scriptContent);

            var newResult = new ScriptAnalysis
            {
                ScriptId = scriptId,
                AnalysisResult = result,
                AnalyzedAt = DateTime.Now
            };

            await _repository.AddOrUpdateAsync(newResult);
            return newResult;
        }

        public async Task<ScriptAnalysis?> GetByScriptIdAsync(int scriptId)
        {
            return await _repository.GetByScriptIdAsync(scriptId);
        }
    }
}
