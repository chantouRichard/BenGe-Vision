using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using picture_backend.DTO;
using picture_backend.Entity;
using picture_backend.Models;
using picture_backend.Services.IServices;
using System.Security.Claims;
using System.Text;

namespace picture_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScriptController: ControllerBase
    {
        private readonly IScriptService _scriptService;
        private readonly IAIService _aiService;


        public ScriptController(IScriptService scriptService, IAIService aiService)
        {
            _scriptService = scriptService;
            _aiService = aiService;
        }


        [HttpPost("directions")]
        public async Task<ActionResult> GenerateSlogan([FromBody] SloganRequestEntity request)
        {
            var result = await _aiService.GenerateSloganAsync(request);
            if (result == null)
            {
                return BadRequest("标语生成失败");
            }
            return Ok(result);
        }
        [HttpPut("directions/stream")]
        public async Task<IActionResult> StreamGenerateSlogan([FromBody] SloganRequestEntity request)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            try
            {
                // 将流式生成的结果通过Server-Sent Events发送
                await foreach (var slogan in _aiService.GenerateSloganStreamAsync(request))
                {
                    // 把每个标语序列化为JSON并发送为SSE事件
                    string serializedSlogan = JsonConvert.SerializeObject(slogan);
                    await Response.WriteAsync($"data: {serializedSlogan}\n\n");
                    await Response.Body.FlushAsync();
                }

                // 发送结束标记
                await Response.WriteAsync("data: [DONE]\n\n");
                await Response.Body.FlushAsync();

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                // 异常情况下尝试发送错误信息
                try
                {
                    await Response.WriteAsync($"data: {{\"error\": \"{ex.Message}\"}}\n\n");
                    await Response.Body.FlushAsync();
                }
                catch
                {
                    // 忽略写入响应中的错误
                }
                return new EmptyResult();
            }
        }
        [HttpPut("directions/stream-complete")]
        public async Task<ActionResult> StreamCompleteGenerateSlogan([FromBody] SloganRequestEntity request)
        {
            var result = await _aiService.StreamToSloganResponseDtoAsync(request);
            return Ok(result);
        }


        // 用户选择剧本,传入剧本ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetScriptById(int id)
        {
            var scriptDetailDto = await _scriptService.GetScriptByIdAsync(id);
            if (scriptDetailDto == null)
            {
                return NotFound("剧本在数据库中不存在或者已经删除");
            }
            return Ok(scriptDetailDto);
        }

        // 根据用户ID获取剧本列表
        [HttpGet("user")]
        public async Task<IActionResult> GetByUserId()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var scripts = await _scriptService.GetUserScriptsAsync(userId);
            return Ok(scripts);
        }

        // 删除某一个剧本内容,根据ID号
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScript(int id)
        {
            await _scriptService.DeleteScriptAsync(id);
            return NoContent();
        }

        // 新建剧本
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateNewScript()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);


            var result = await _scriptService.InitializeScriptAsync(userId);

            return Ok(result);
        }
        
        // 第二阶段对话
        //[Authorize]
        [HttpPost("reply2nd")]
        public async Task<IActionResult> HandleUserMessage2nd([FromBody] ScriptReplyRequestEntity request)
        {
            if (string.IsNullOrWhiteSpace(request.Message) || request.ScriptId <= 0)
            {
                return BadRequest("修改的剧本或者用户输入的消息不能为空");
            }

            ScriptDetailDto? scriptdetaildto = await _scriptService.GetScriptByIdAsync(request.ScriptId);

            if (scriptdetaildto == null)
            {
                return BadRequest("剧本在数据库中不存在或者已经删除");
            }
            var scriptFrameworkDto = await _scriptService.GenFrame(request, scriptdetaildto.History, scriptdetaildto.Script.Content);

            if (scriptFrameworkDto == null)
            {
                return BadRequest("API请求失败");
            }
            return Ok(scriptFrameworkDto);
        }

        // 第二阶段对话流式生成
        [HttpPut("reply2nd/stream")]
        public async Task<IActionResult> StreamHandleUserMessage2nd([FromBody] ScriptReplyRequestEntity request)
        {
            if (string.IsNullOrWhiteSpace(request.Message) || request.ScriptId <= 0)
            {
                return BadRequest("修改的剧本或者用户输入的消息不能为空");
            }

            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            try
            {
                await foreach (var chunk in _scriptService.GenFrameStreamAsync(request))
                {
                    // 将每一部分内容通过 SSE 发送到客户端
                    await Response.WriteAsync($"data: {chunk}\n\n");
                    await Response.Body.FlushAsync();
                }

                // 发送结束标记
                await Response.WriteAsync("data: [DONE]\n\n");
                await Response.Body.FlushAsync();

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                // 异常处理
                await Response.WriteAsync($"data: {{\"error\": \"{ex.Message}\"}}\n\n");
                await Response.Body.FlushAsync();
                return new EmptyResult();
            }
        }

        // 根据剧本框架生成完整剧本内容
        [Authorize]
        [HttpPost("reply3rd")]
        public async Task<IActionResult> HandleUserMessage3rd([FromBody] ScriptReplyRequestEntity request)
        {
            if (request.ScriptId <= 0)
            {
                return BadRequest("修改的剧本不能为空");
            }
            ScriptDetailDto?scriptdto = await _scriptService.GetScriptByIdAsync(request.ScriptId);
            
            if (scriptdto == null)
            {
                return NotFound("剧本在数据库中不存在或者已经删除");
            }
            var scriptDetailDto = await _scriptService.GetCompSctiptAndDesc(scriptdto.Script);
            return Ok(scriptDetailDto);
        }

        // 剧本框架分析
        [Authorize]
        [HttpPut("analyze")]
        public async Task<IActionResult> AnalyzeScriptContent([FromBody] ScriptReplyRequestEntity request)
        {
            if (string.IsNullOrWhiteSpace(request.Message) || request.ScriptId <= 0)
            {
                return BadRequest("请求体不能为空");
            }

            var scriptAnalysis = await _scriptService.AnalyzeScriptContent(request.Message, request.ScriptId);
            if(scriptAnalysis == null)
                return BadRequest("API请求失败");
            return Ok(scriptAnalysis);
        }

        // 从第一阶段进入第二阶段
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateScript([FromBody] ScriptUpdateRequestEntity request)
        {
            if (string.IsNullOrWhiteSpace(request.Content) || request.ScriptId <= 0 || request.Stage <= 0)
            {
                return BadRequest("剧本ID、和内容不能为空, 阶段错误");
            }
            await _scriptService.UpdateScriptAsync(request.ScriptId, "新剧本", request.Content, request.Stage);
            var scriptDetailDto = await _scriptService.GetScriptByIdAsync(request.ScriptId);
            return Ok(scriptDetailDto);
        }

        // 选择生成图像的可视化元素
        [Authorize]
        [HttpPut("visualize")]
        public async Task<IActionResult> VisualSelectedElement([FromBody] ScriptVisualRequestEntity request)
        {
            if (request.ElementId <= 0 || request.ScriptId <= 0)
            {
                return BadRequest("剧本ID、和元素ID不能为空");
            }
            var urlPath = await _scriptService.VisualizeScriptAsync(request.ScriptId, request.ElementId);
            return Ok(urlPath);
        }
        [HttpPost("chat/stream")]
        public async Task<IActionResult> ChatStream([FromBody] ChatRequestEntity request)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            try
            {
                await foreach (var chunk in _aiService.ChatStreamAsync(request))
                {
                    await Response.WriteAsync($"data: {chunk}\n\n");
                    await Response.Body.FlushAsync();
                }
                await Response.WriteAsync("data: [DONE]\n\n");
                await Response.Body.FlushAsync();
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                await Response.WriteAsync($"data: {{\"error\": \"{ex.Message}\"}}\n\n");
                await Response.Body.FlushAsync();
                return new EmptyResult();
            }
        }


    }
}
