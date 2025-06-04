namespace picture_backend.Entity
{
    public class SloganRequestEntity
    {
        public string Prompt { get; set; } = string.Empty; // 用户输入的提示
        public int ScriptId { get; set; } // Slogan的 ID
    }
}
