using picture_backend.Models;
using picture_backend.Repositories.IRepositories;
using picture_backend.Services.IServices;

namespace picture_backend.Services
{
    public class VisualElementService : IVisualElementService
    {
        private readonly IVisualElementRepository _repository;

        public VisualElementService(IVisualElementRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<VisualElement>> GetAllElementsAsync(int scriptId)
        {
            return await _repository.GetAllByScriptIdAsync(scriptId);
        }

        public async Task UpdateVisualElementsAsync(int scriptId, List<VisualElement> newElements)
        {
            await _repository.ReplaceElementsAsync(scriptId, newElements);
        }
        public async Task<VisualElement> GetElementByIdAsync(int elementId)
        {
            return await _repository.GetByIdAsync(elementId);
        }
        public async Task UpdateElementUrlAsync(int elementId, string url)
        {
            var element = await _repository.GetByIdAsync(elementId);
            if (element != null)
            {
                element.ImageUrl = url;
                await _repository.UpdateAsync(element);
            }
        }
    }
}
