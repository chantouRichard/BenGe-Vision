using picture_backend.Services.IServices;
using picture_backend.Repositories.IRepositories;
using picture_backend.Models;
using picture_backend.Repositories;
using picture_backend.DTO;
using System;
using picture_backend.Entity;
using System.Data.SqlTypes;

namespace picture_backend.Services
{
    public class ScriptService : IScriptService
    {
        private readonly IScriptRepository _scriptRepository;
        private readonly IScriptAnalysisService _scriptAnalysisService;
        private readonly IVisualElementService _visualElementService;
        private readonly IScriptHistoryService _scriptHistoryService;
        private readonly IAIService _aiservice;

        public ScriptService(IScriptRepository scriptRepository, IScriptAnalysisService scriptAnalysisService, IVisualElementService visualElementService, IScriptHistoryService scriptHistoryService, IAIService aiservice)
        {
            this._scriptRepository = scriptRepository;
            this._scriptAnalysisService = scriptAnalysisService;
            this._visualElementService = visualElementService;
            this._scriptHistoryService = scriptHistoryService;
            this._aiservice = aiservice;
        }

        public async Task<ScriptDetailDto?> GetScriptDetailAsync(int scriptId)
        {
            var script = await _scriptRepository.GetByIdAsync(scriptId);
            if (script == null || script.IsDeleted)
                return null;

            var history = await _scriptHistoryService.GetHistoryByScriptAsync(scriptId);
            var analysis = await _scriptAnalysisService.GetByScriptIdAsync(scriptId);
            var visualElements = await _visualElementService.GetAllElementsAsync(scriptId);

            return new ScriptDetailDto
            {
                Script = script,
                History = history,
                Analysis = analysis,
                VisualElements = visualElements
            };
        }

        public async Task<ScriptDetailDto?> GetScriptByIdAsync(int id)
        {
            return await GetScriptDetailAsync(id);
        }
        public async Task<IEnumerable<Script>> GetUserScriptsAsync(int userId) => await _scriptRepository.GetByUserIdAsync(userId);
        public async Task<Script> CreateScriptAsync(Script script) => await _scriptRepository.AddAsync(script);
        public async Task UpdateScriptAsync(int scriptId, string title, string content, int stage) => await _scriptRepository.UpdateAsync(scriptId, title, content, stage);
        public async Task DeleteScriptAsync(int id) => await _scriptRepository.DeleteAsync(id);

        public async Task<ScriptDetailDto> InitializeScriptAsync(int userId)
        {
            var newScript = new Script
            {
                Title = "新剧本",
                Content = String.Empty,
                UserId = userId,
                IsDeleted = false,
                LastUpdated = DateTime.Now,
                Stage = 1
            };
            await _scriptRepository.AddAsync(newScript);

            return new ScriptDetailDto
            {
                Script = newScript,
                History = new List<ScriptHistory>(),
                Analysis = null,
                VisualElements = new List<VisualElement>()
            };
        }


        // 模拟调用AI回复用户内容
        public async Task<ScriptDetailDto?> HandleUserMessage(string userMessage, int scriptId)
        {
            var response = " \"背景\": \"在太平洋航行的豪华游轮[爱神号]上，正举行珠宝大亨千金的婚礼。仪式开始前15分钟，新娘突然从化妆室消失，只留下地板上未干的血迹。游轮还有1小时即将起航，所有宾客都成为了嫌疑人......\",\r\n  \"人物剧本\": [\r\n    \"【伴娘琳达】新娘的大学室友，三个月前因投资失败向新娘借款50万未还，最近频繁收到催债短信\",\r\n    \"【新郎艾文】出身普通家庭的证券分析师，婚礼前一周银行账户突然多出200万转账\",\r\n    \"【新娘妹妹露西】艺术系学生，随身画本里夹着张奇怪的轮船结构图，图中某处画着红色叉号\",\r\n    \"【船长安东尼】服务爱神号二十年的老船员，制服用金线绣着特别的船锚图案，口袋里装着备用舱门钥匙\",\r\n    \"【闺蜜苏珊】新娘的童年玩伴，手机里存着多条未发送的信息草稿：[其实我一直...]\"\r\n  ],\r\n  \"线索\": [\r\n    \"化妆台抽屉里的空药瓶，标签显示是镇静类药物\",\r\n    \"甲板角落发现撕碎的匿名威胁信碎片，拼出[取消婚礼]字样\",\r\n    \"新娘更衣室挂着的婚纱腰封内侧有轻微撕裂\",\r\n    \"轮机舱工具箱底部沾有口红印的扳手\",\r\n    \"宴会厅监控显示08:45有人影从消防通道闪过\"\r\n  ],\r\n  \"真相\": [\r\n    \"新娘因发现苏珊与艾文的私情要求取消婚礼，争执中被苏珊用扳手误伤\",\r\n    \"伴娘琳达在茶水下安眠药想制造新娘逃婚假象，却撞见昏迷的新娘\",\r\n    \"船长将受伤的新娘藏进底舱密室（结构图叉号处），用备用钥匙反锁舱门\",\r\n    \"婚纱撕裂是因新娘挣扎时勾住消防斧，斧头现藏在救生艇储物箱内\",\r\n    \"匿名信是露西为阻止姐姐婚姻所写，200万来自她变卖母亲遗物所得\"\r\n  ],\r\n  \"组织者手册\": \"重点引导新人关注时间线矛盾点：①08:40苏珊离开化妆室时长 ②琳达补妆时镜面反射异常 ③船长制服图案与密室图腾一致。分三轮搜证：首轮基础线索，次轮深入物品关联，末轮触发血迹检测报告。注意通过音乐控制节奏，游轮鸣笛声作为阶段提示。最后揭秘时演示如何通过婚纱纤维与扳手凹槽匹配锁定凶器。\""; // 这里需要调用AI的接口获取回复内容
            var title = "消失的新娘";
            // 更改剧本的标题和内容

            await UpdateScriptAsync(scriptId, title, response, 2);  //更新数据库中剧本数据

            var userHistory = new ScriptHistory
            {
                ScriptId = scriptId,
                Message = userMessage,
                Response = string.Empty,
                CreatedAt = DateTime.Now
            };

            await _scriptHistoryService.AddHistoryAsync(userHistory);  //添加用户消息历史记录

            var mockAIHistory = new ScriptHistory
            {
                ScriptId = scriptId,
                Message = string.Empty,
                Response = response,
                CreatedAt = DateTime.Now
            };

            await _scriptHistoryService.AddHistoryAsync(mockAIHistory); //添加AI返回消息历史记录

            // 可视化元素描述的更新 todo

            return await GetScriptByIdAsync(scriptId);
        }
        public string ScriptJsonToMarkdown(Detail detail) {  //格式转换Detail -> MarkDown
            string retscript = "# " + detail.Title + "\n\n---\n\n## 背景\n" + detail.Background + "\n\n---\n\n## 人物剧本\n\n";
            foreach (string s in detail.chrScript)
            {
                retscript += "- CHR " + s + "\n";
            }
            retscript = retscript + "\n---\n\n## 线索\n\n";
            foreach (string s in detail.Clues)
            {
                retscript += "- " + s + "\n";
            }
            retscript = retscript + "\n---\n\n## 真相\n\n- " + detail.Truths + "\n";

            retscript = retscript + "\n---\n\n## 组织者手册\n\n";
            retscript = retscript + detail.DMBook;
            return retscript;

        }


        public async Task<ScriptFrameworkDto?> GenFrame(ScriptReplyRequestEntity request, List<ScriptHistory> History, string scriptcontent)   //调用AI生成大致框架
        {
            Detail detail;
            List<Message> msgs = new List<Message>();
            if (History.Count == 0)
            {
                msgs.Add(new Message { Role = "user", Content = request.Message + "\n以下是剧本创作的方向：\n" + scriptcontent });
            }
            else
            {
                for (int i = 0; i < History.Count; i += 1)
                {
                    if (History[i].Message == string.Empty)
                    {
                        continue;
                    }
                    msgs.Add(new Message { Role = "user", Content = History[i].Message });
                    msgs.Add(new Message { Role = "assistant", Content = scriptcontent });
                }
                msgs.Add(new Message { Role = "user", Content = request.Message });
            } 
            try
            {
                Task<Detail> task = _aiservice.GenFramework(msgs);
                detail = await task;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API请求失败: {ex.Message}");
                return null;
            }


            var title = detail.Title;

            string retscript = ScriptJsonToMarkdown(detail);

            await UpdateScriptAsync(request.ScriptId, detail.Title, retscript, 2);  //更新数据库中剧本数据

            var userHistory = new ScriptHistory
            {
                ScriptId = request.ScriptId,
                Message = request.Message,
                Response = string.Empty,
                CreatedAt = DateTime.Now
            };

            await _scriptHistoryService.AddHistoryAsync(userHistory);  //添加用户消息历史记录

            var AIHistory = new ScriptHistory
            {
                ScriptId = request.ScriptId,
                Message = string.Empty,
                Response = detail.describe,
                CreatedAt = DateTime.Now
            };

            await _scriptHistoryService.AddHistoryAsync(AIHistory);  //添加AI消息历史记录

            var scriptdetaildto = await GetScriptByIdAsync(request.ScriptId);

            return new ScriptFrameworkDto
            {
                script = scriptdetaildto.Script,
                dialogHistory = scriptdetaildto.History
            };
        }

        public async IAsyncEnumerable<string> GenFrameStreamAsync(ScriptReplyRequestEntity request)
        {
            // 模拟调用 GPT-3.5 流式 API
            for (int i = 1; i <= 5; i++)
            {
                await Task.Delay(500); // 模拟延迟
                yield return $"GPT-3.5 流式响应第 {i} 部分";
            }
        }


        public async Task<ScriptAnalysis?> AnalyzeScriptContent(string scriptContent, int scriptId)
        {
            var newAnalysis = await _scriptAnalysisService.AnalyzeScriptAsync(scriptId, scriptContent);
            return newAnalysis;
        }

        public async Task<ScriptDetailDto?> GetCompSctiptAndDesc(Script script)
        {
            string detail = await _aiservice.GenDetail(script.Content, script.Title);
            // 重新设计后的imageMsgs没有url
            ImageMsgs imageMsgs = await _aiservice.GenDescription(detail);

            await UpdateScriptAsync(script.Id, script.Title, detail, 3);  //更新数据库中剧本数据

            if (imageMsgs == null)  //描述词生成失败
            {
                var scriptdetaild = await GetScriptByIdAsync(script.Id);
                return scriptdetaild;
            }

            List<VisualElement> visualElements = new List<VisualElement>();
            for (int i = 0; i < imageMsgs.thingDesc[0].Name.Count; i += 1)
            {
                visualElements.Add(new VisualElement
                {
                    Type = "Character",
                    Name = imageMsgs.thingDesc[0].Name[i],
                    Description = imageMsgs.thingDesc[0].Desc[i],
                    ImageUrl = String.Empty,
                    ImageGeneratedAt = DateTime.Now,
                    ScriptId = script.Id,
                });
            }
            for (int i = 0; i < imageMsgs.thingDesc[1].Name.Count; i += 1)
            {
                visualElements.Add(new VisualElement
                {
                    Type = "Scene",
                    Name = imageMsgs.thingDesc[1].Name[i],
                    Description = imageMsgs.thingDesc[1].Desc[i],
                    ImageUrl = String.Empty,
                    ImageGeneratedAt = DateTime.Now,
                    ScriptId = script.Id,
                });
            }
            for (int i = 0; i < imageMsgs.thingDesc[2].Name.Count; i += 1)
            {
                visualElements.Add(new VisualElement
                {
                    Type = "Prop",
                    Name = imageMsgs.thingDesc[2].Name[i],
                    Description = imageMsgs.thingDesc[2].Desc[i],
                    ImageUrl = String.Empty,
                    ImageGeneratedAt = DateTime.Now,
                    ScriptId = script.Id,
                });
            }
            await _visualElementService.UpdateVisualElementsAsync(script.Id, visualElements);  //更新数据库中visual element描述词部分
            var scriptdetaildto = await GetScriptByIdAsync(script.Id);

            return scriptdetaildto;
        }

        public async Task<string> VisualizeScriptAsync(int scriptId, int elementId)
        {
            var visualElement = await _visualElementService.GetElementByIdAsync(elementId);
            if (visualElement == null)
                return "元素不存在";
            var imageUrl = await _aiservice.GenerateImageUrl(visualElement.Type, visualElement.Name, visualElement.Description);
            if (imageUrl == null)
                return "图片生成失败";
            await _visualElementService.UpdateElementUrlAsync(visualElement.Id, imageUrl);
            return imageUrl;
        }

    }
}
