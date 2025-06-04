using picture_backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace picture_backend.Services.IServices
{
    public interface IScriptHistoryService
    {
        Task AddHistoryAsync(ScriptHistory history);
        Task<List<ScriptHistory>> GetHistoryByScriptAsync(int scriptId);
        //Task<List<ScriptHistory>> GetScriptHistoryAsync(int scriptId);
        //Task UpdateLatestAsync(int scriptId, string userMessage, string response);


    }
}
