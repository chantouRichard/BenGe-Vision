using picture_backend.Repositories.IRepositories;
using picture_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace picture_backend.Repositories
{
    public class ScriptRepository : IScriptRepository
    {
        private readonly AppDbContext _context;

        public ScriptRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Script> GetByIdAsync(int id)
        {
            return await _context.Scripts.Include(s => s.ScriptAnalysis)
                                         .Include(s => s.VisualElements)
                                         .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<IEnumerable<Script>> GetByUserIdAsync(int userId) =>
        await _context.Scripts.Where(s => s.UserId == userId && !s.IsDeleted).ToListAsync();

        public async Task<Script> AddAsync(Script script)
        {
            _context.Scripts.Add(script);
            await _context.SaveChangesAsync();
            return script;
        }

        public async Task<bool> UpdateAsync(int scriptId, string title, string content, int stage)
        {
            var script = await _context.Scripts.FirstOrDefaultAsync(s => s.Id == scriptId && !s.IsDeleted);
            if (script != null)
            {
                script.Title = title;
                script.Content = content;
                script.LastUpdated = DateTime.Now;
                script.Stage = stage;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task DeleteAsync(int id)
        {
            var script = await _context.Scripts.FindAsync(id);
            if (script != null)
            {
                script.IsDeleted = true;
                script.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
