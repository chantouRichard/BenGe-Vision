using Microsoft.EntityFrameworkCore;
using picture_backend.Models;
using picture_backend.Repositories.IRepositories;

namespace picture_backend.Repositories
{
    public class ScriptAnalysisRepository : IScriptAnalysisRepository
    {
        private readonly AppDbContext _context;

        public ScriptAnalysisRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ScriptAnalysis?> GetByScriptIdAsync(int scriptId)
        {
            return await _context.ScriptAnalyses.FirstOrDefaultAsync(sa => sa.ScriptId == scriptId);
        }

        public async Task AddOrUpdateAsync(ScriptAnalysis analysis)
        {
            var existing = await _context.ScriptAnalyses.FirstOrDefaultAsync(sa => sa.ScriptId == analysis.ScriptId);
            if (existing != null)
            {
                existing.AnalysisResult = analysis.AnalysisResult;
                existing.AnalyzedAt = analysis.AnalyzedAt;
            }
            else
            {
                _context.ScriptAnalyses.Add(analysis);
            }
            await _context.SaveChangesAsync();
        }
    }

}
