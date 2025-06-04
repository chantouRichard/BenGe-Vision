using picture_backend.DTO;
using picture_backend.Entity;
using picture_backend.Models;

namespace picture_backend.Services.IServices
{
    public interface IAIService
    {
        Task<SloganResponseDto?> GenerateSloganAsync(SloganRequestEntity request);
        IAsyncEnumerable<Slogan> GenerateSloganStreamAsync(SloganRequestEntity request);
        Task<SloganResponseDto> StreamToSloganResponseDtoAsync(SloganRequestEntity request);


        Task<Detail> GenFramework(List<Message> msgs);

        Task<String> GenDetail(string frame, string title);
        Task<string> AnalyzeScriptContent(string scriptContent);
        Task<string> GenerateImageUrl(string type, string name, string imageDesc);

        Task<ImageMsgs> GenDescription(string final);
        IAsyncEnumerable<string> ChatStreamAsync(ChatRequestEntity request);
    }
}
