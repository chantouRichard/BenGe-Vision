using System.Text.Json.Serialization;

namespace picture_backend.Models
{
    public class ScriptHistory
    {
        public int Id { get; set; }
        public string? Message { get; set; }  //用户发给AI的消息
        public string? Response { get; set; }  //AI与用户交互的对话
        public DateTime CreatedAt { get; set; }

        public int ScriptId { get; set; }
        [JsonIgnore]
        public Script Script { get; set; } = null!;  //剧本框架（markdown版）
    }

}
