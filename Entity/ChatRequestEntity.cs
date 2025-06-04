using System.Text.Json.Serialization;

namespace picture_backend.Entity
{
    public class ChatRequestEntity
    {
        public string Message { get; set; }      // 当前用户输入
        public List<string> History { get; set; } // 可选：历史对话内容
    }

}