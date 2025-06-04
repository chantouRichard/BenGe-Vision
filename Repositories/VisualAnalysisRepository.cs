using Microsoft.EntityFrameworkCore;
using picture_backend.Models;
using picture_backend.Repositories.IRepositories;

namespace picture_backend.Repositories
{
    public class VisualElementRepository : IVisualElementRepository
    {
        private readonly AppDbContext _context;

        public VisualElementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VisualElement>> GetAllByScriptIdAsync(int scriptId)
        {
            return await _context.VisualElements
                .Where(v => v.ScriptId == scriptId)
                .ToListAsync();
        }

        public async Task ReplaceElementsAsync(int scriptId, List<VisualElement> elements)
        {
            var existing = await _context.VisualElements
                .Where(v => v.ScriptId == scriptId)
                .ToListAsync();

            _context.VisualElements.RemoveRange(existing);
            await _context.SaveChangesAsync();

            foreach (var e in elements)
            {
                e.ScriptId = scriptId;
            }

            _context.VisualElements.AddRange(elements);
            await _context.SaveChangesAsync();
        }
        public async Task<VisualElement> GetByIdAsync(int elementId)
        {
            return await _context.VisualElements
                .FirstOrDefaultAsync(v => v.Id == elementId);
        }
        public async Task UpdateAsync(VisualElement element)
        {
            _context.VisualElements.Update(element);
            await _context.SaveChangesAsync();
        }
    }

}
