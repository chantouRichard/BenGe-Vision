namespace picture_backend.DTO
{
    public class Slogan
    {
        public string Content { get; set; } // 标语内容
        public string CoreIdea { get; set; } // 核心创意
    }

    public class SloganResponseDto
    {
        public List<Slogan> Slogans { get; set; } = new List<Slogan>();
    }

}
