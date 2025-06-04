using Microsoft.EntityFrameworkCore;
using picture_backend.Models;
using picture_backend.Repositories.IRepositories;
using picture_backend.Services.IServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace picture_backend.Services
{
    public class ScriptHistoryService : IScriptHistoryService
    {
        private readonly IScriptHistoryRepository _historyRepo;

        public ScriptHistoryService(IScriptHistoryRepository historyRepo)
        {
            _historyRepo = historyRepo;
        }

        public async Task AddHistoryAsync(ScriptHistory history)
        {
            await _historyRepo.AddAsync(history);
        }

        public async Task<List<ScriptHistory>> GetHistoryByScriptAsync(int scriptId)
        {
            return await _historyRepo.GetByScriptIdAsync(scriptId);
        }

        //public async Task UpdateLatestAsync(int scriptId, string userMessage, string response)
        //{
        //    // 使用_historyRepo而不是_context
        //    await _historyRepo.UpdateLatestAsync(scriptId, userMessage, response);
        //}
        //public async Task<List<ScriptHistory>> GetScriptHistoryAsync(int scriptId)
        //{
        //    // 可以直接调用已有的方法，保持逻辑一致
        //    return await GetHistoryByScriptAsync(scriptId);
        //}
    }
}

