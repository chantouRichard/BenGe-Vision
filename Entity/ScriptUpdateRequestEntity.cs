namespace picture_backend.Entity
{
    public class ScriptUpdateRequestEntity
    {
        public int ScriptId { get; set; }
        public string? Content { get; set; }
        public int Stage { get; set; } = 1;
    }
}
