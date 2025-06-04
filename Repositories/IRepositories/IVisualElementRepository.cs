using picture_backend.Models;

namespace picture_backend.Repositories.IRepositories
{
    public interface IVisualElementRepository
    {
        Task<List<VisualElement>> GetAllByScriptIdAsync(int scriptId);
        Task ReplaceElementsAsync(int scriptId, List<VisualElement> elements);
        Task<VisualElement> GetByIdAsync(int elementId);
        Task UpdateAsync(VisualElement element);
    }
}
