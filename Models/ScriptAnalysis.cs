using System.Text.Json.Serialization;

namespace picture_backend.Models
{
    public class ScriptAnalysis
    {
        public int Id { get; set; }
        public string? AnalysisResult { get; set; }
        public DateTime AnalyzedAt { get; set; }

        public int ScriptId { get; set; }
        [JsonIgnore]
        public Script Script { get; set; } = null!;
    }

}
