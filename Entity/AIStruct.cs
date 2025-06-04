using System.Text.Json.Serialization;

namespace picture_backend.Entity
{

    //stage2
    public class DeepSeekResponse
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("created")]
        public long Created { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("choices")]
        public List<ChoiceMessage> Choices { get; set; }
    }
    public class ChoiceMessage
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }
        [JsonPropertyName("message")]
        public Message Mes { get; set; }
    }
    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class DialogBox
    {
        public string scriptSummary { get; set; }
        public string fullScriptContent { get; set; }
        public List<History> history { get; set; }
        public string instruction { get; set; }
    }
    public class History
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class Detail
    {
        [JsonPropertyName("作为作家的礼貌性回答")]
        public string describe { get; set; }
        [JsonPropertyName("标题")]
        public string Title { get; set; }
        [JsonPropertyName("背景")]
        public string Background { get; set; }
        [JsonPropertyName("人物剧本")]
        public List<string> chrScript { get; set; }
        [JsonPropertyName("线索")]
        public List<string> Clues { get; set; }
        [JsonPropertyName("真相")]
        public string Truths { get; set; }
        [JsonPropertyName("组织者手册")]
        public string DMBook { get; set; }
    }
    public class LessDetail
    {
        [JsonPropertyName("标题")]
        public string Title { get; set; }
        [JsonPropertyName("背景")]
        public string Background { get; set; }
        [JsonPropertyName("人物剧本")]
        public List<string> chrScript { get; set; }
        [JsonPropertyName("线索")]
        public List<string> Clues { get; set; }
        [JsonPropertyName("真相")]
        public string Truths { get; set; }
        [JsonPropertyName("组织者手册")]
        public string DMBook { get; set; }
    }
    public class Ctitle
    {
        [JsonPropertyName("标题")]
        public string Title { get; set; }
    }
    public class Cbackground
    {
        [JsonPropertyName("背景")]
        public string Background { get; set; }
    }
    public class CchrScript
    {
        [JsonPropertyName("人物剧本")]
        public List<string> chrScript { get; set; }
    }
    public class Cclue
    {
        [JsonPropertyName("线索")]
        public List<string> Clues { get; set; }
    }
    public class Ctrue
    {
        [JsonPropertyName("真相")]
        public string Truths { get; set; }
    }
    public class Cdmbook
    {
        [JsonPropertyName("组织者手册")]
        public string DMBook { get; set; }
    }

    public class ContentData
    {
        public string Story_summary { get; set; }
        public List<Character> Characters { get; set; }
        public List<Puzzle> Puzzles { get; set; }
        public List<string> Player_interactions { get; set; }
    }

    public class Character
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Motivation { get; set; }
        public string Behavior_logic { get; set; }
    }

    public class Puzzle
    {
        public string Description { get; set; }
        public string Difficulty { get; set; }
        public bool Clues_provided { get; set; }
    }
    public class AIAnalysisResult
    {
        public AnalysisData Analysis { get; set; }

        public class AnalysisData
        {
            public List<string> Point { get; set; }    // 亮点
            public List<string> Difficulty { get; set; } // 难点
            public List<string> Suggestion { get; set; } // 建议
            public ScoreData Score { get; set; }        // 评分
        }

        public class ScoreData
        {
            public int Logicality { get; set; }  // 逻辑性
            public int Storiness { get; set; }   // 故事性
            public int Experience { get; set; }  // 体验感
        }
    }

    public class ImageMsgs
    {
        public List<ThingDesc> thingDesc { get; set; }
    }
    public class ThingDesc  //特征提取
    {
        [JsonPropertyName("名称")]
        public List<string> Name { get; set; }
        [JsonPropertyName("描绘")]
        public List<string> Desc { get; set; }
    }
    public class ImageParameters  //图片生成
    {
        public string size { get; set; } = "1024*1024";
        public int n { get; set; } = 1;
        public bool prompt_extend { get; set; } = true;
        public bool watermark { get; set; } = false;
    }
}
