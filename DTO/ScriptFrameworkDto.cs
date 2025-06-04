using picture_backend.Models;

namespace picture_backend.DTO
{
    public class ScriptFrameworkDto //第二部分返回前端的接口
    {
        public Script script { get; set; } = null!;  //当前的剧本（markdown版）
        public List<ScriptHistory> dialogHistory { get; set; } = new();  //剧本的历史（markdown版） + 与AI对话历史
    }
}