using System.ComponentModel.DataAnnotations;

namespace picture_backend.Entity
{
    /// <summary>
    /// ��ʽ���ɾ籾��ܵ�����ʵ��
    /// </summary>
    public class ScriptFrameworkRequestEntity
    {
        /// <summary>
        /// �籾ID
        /// </summary>
        [Required]
        public int ScriptId { get; set; }

        /// <summary>
        /// �û�������ʾ����ѡ��
        /// </summary>
        public string? UserPrompt { get; set; }
    }
}