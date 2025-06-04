using picture_backend.Models;

namespace picture_backend.Services.IServices
{
    public interface IVisualElementService
    {
        Task<List<VisualElement>> GetAllElementsAsync(int scriptId);
        Task UpdateVisualElementsAsync(int scriptId, List<VisualElement> newElements);
        Task<VisualElement> GetElementByIdAsync(int elementId);
        Task UpdateElementUrlAsync(int elementId, string url);
    }
}
