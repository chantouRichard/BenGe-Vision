using System.Text.Json.Serialization;

namespace picture_backend.Models
{
    public class Script
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime LastUpdated { get; set; }

        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; } = null!;
        
        public ICollection<ScriptHistory> ScriptHistories { get; set; } = new List<ScriptHistory>();
        public ICollection<VisualElement> VisualElements { get; set; } = new List<VisualElement>();
        public ScriptAnalysis? ScriptAnalysis { get; set; }
        public int Stage { get; set; } = 1;
    }

}
