using Microsoft.EntityFrameworkCore;
using picture_backend.Models;
using picture_backend.Repositories.IRepositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace picture_backend.Repositories
{
    public class ScriptHistoryRepository : IScriptHistoryRepository
    {
        private readonly AppDbContext _context;

        public ScriptHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ScriptHistory history)
        {
            await _context.ScriptHistories.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ScriptHistory>> GetByScriptIdAsync(int scriptId)
        {
            return await _context.ScriptHistories
                .Where(h => h.ScriptId == scriptId)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();
        }

    }
}
