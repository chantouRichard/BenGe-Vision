using System.Text.Json.Serialization;

namespace picture_backend.Models
{
    public class VisualElement
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!; // Character / Scene / Prop 
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? ImageGeneratedAt { get; set; }

        public int ScriptId { get; set; }
        [JsonIgnore]
        public Script Script { get; set; } = null!;
    }

}
