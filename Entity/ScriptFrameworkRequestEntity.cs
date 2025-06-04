using System.ComponentModel.DataAnnotations;

namespace picture_backend.Entity
{
    /// <summary>
    /// 流式生成剧本框架的请求实体
    /// </summary>
    public class ScriptFrameworkRequestEntity
    {
        /// <summary>
        /// 剧本ID
        /// </summary>
        [Required]
        public int ScriptId { get; set; }

        /// <summary>
        /// 用户附加提示（可选）
        /// </summary>
        public string? UserPrompt { get; set; }
    }
}