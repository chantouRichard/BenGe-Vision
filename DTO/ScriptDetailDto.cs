using picture_backend.Models;

namespace picture_backend.DTO
{
    public class ScriptDetailDto
    {
        public Script Script { get; set; } = null!;
        public List<ScriptHistory> History { get; set; } = new();
        public ScriptAnalysis? Analysis { get; set; }
        public List<VisualElement> VisualElements { get; set; } = new();
    }
}
