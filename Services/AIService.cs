using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using picture_backend.DTO;
using picture_backend.Entity;
using picture_backend.Services.IServices;
using System.Text.Json;
using System.Reflection.Metadata;


namespace picture_backend.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerSettings _jsonSettings;

        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://api.deepseek.com/";
        private const string PICTURE_API_KEY = "sk-817d27e9c883406a87d98eb23fc4b1a9";
        private static readonly string Picture_Url = "https://dashscope.aliyuncs.com";

        public AIService(HttpClient client)
        {
            //stage1
            _client = client;
            _client.BaseAddress = new Uri("https://api.chatanywhere.tech");
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer sk-itFhufoh4wQKGkvyfiFE7BjyzL4aZAjMMqJIKQZWAwsULDm9");

            _jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            //stage2
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(6);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer sk-8dbe5de96f5644849250648772aff5e2");
        }

        public async Task<SloganResponseDto?> GenerateSloganAsync(SloganRequestEntity request)
        {
            try
            {
                var requestData = new
                {
                    messages = new[]
                    {
                new { role = "system",
                    content = "你是一个剧本杀广告生成者，" +
                    "请严格按照以下格式生成内容：" +
                    "剧本背景: ...\n玩家目标: ...\n核心创意: ...\n" +
                    "内容必须完整，字数限制在300字以内。" +
                    "请确保每次生成的内容都具有独特性，但必须严格遵守上述格式。" },
                new { role = "user",
                    content = $"请根据以下关键词生成完整的剧本杀广告" +
                    $"严格按照以下格式生成内容：" +
                    $"剧本背景: ...\n玩家目标: ...\n核心创意: ...\n" +
                    $"每次生成的内容必须独特，可以通过改变背景设定、角色类型、目标描述或核心创意的表达方式来实现。" +
                    $"关键词包括以下几点：{request.Prompt}" }
                    },
                    model = "gpt-3.5-turbo",
                    temperature = 1.0, // 提高随机性
                    top_p = 0.9,       // 增加多样性
                    max_tokens = 600,
                    n = 3
                };

                var requestContent = new StringContent(
                    JsonConvert.SerializeObject(requestData, _jsonSettings),
                    Encoding.UTF8,
                    "application/json");

                var response = await _client.PostAsync("/v1/chat/completions", requestContent);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

                // 将生成的标语转换为 Slogan 对象
                var slogans = responseObject.Choices.Select(choice =>
                {
                    var content = choice.Message.Content?.Trim() ?? string.Empty;
                    var coreIdea = ExtractCoreIdea(content);

                    // 确保 Content 中不包含 CoreIdea
                    if (!string.IsNullOrWhiteSpace(coreIdea))
                    {
                        // 使用正则表达式移除 "核心创意" 部分及其后续内容
                        content = Regex.Replace(content, @"\n\n核心创意[:：].+", string.Empty, RegexOptions.Singleline).Trim();
                    }

                    return new Slogan
                    {
                        Content = content,
                        CoreIdea = coreIdea
                    };
                }).ToList();

                return new SloganResponseDto
                {
                    Slogans = slogans
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"生成标语失败: {ex.Message}");
                return null;
            }
        }

        // 示例核心创意提取方法
        private string ExtractCoreIdea(string? content)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;

            // 使用正则表达式提取 "核心创意" 部分及其后续内容
            var match = Regex.Match(content, @"\n\n核心创意[:：](.+)", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        public async Task<SloganResponseDto> StreamToSloganResponseDtoAsync(SloganRequestEntity request)
        {
            var slogans = new List<Slogan>();

            try
            {
                // 复用现有的流式处理方法
                await foreach (var slogan in GenerateSloganStreamAsync(request))
                {
                    if (slogan != null)
                    {
                        slogans.Add(slogan);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"流式生成标语到DTO失败: {ex.Message}");
                // 出错时也返回已收集的结果，而不是返回null
            }

            return new SloganResponseDto
            {
                Slogans = slogans
            };
        }


        public async IAsyncEnumerable<Slogan> GenerateSloganStreamAsync(SloganRequestEntity request)
        {
            StreamReader reader = null;
            try
            {
                var requestData = new
                {
                    messages = new[]
                    {
                new { role = "system",
                    content = "你是一个剧本杀广告生成者，" +
                    "请严格按照以下格式生成内容：" +
                    "剧本背景: ...\n玩家目标: ...\n核心创意: ...\n" +
                    "内容必须完整，字数限制在300字以内。" +
                    "请确保每次生成的内容都具有独特性，但必须严格遵守上述格式。" },
                new { role = "user",
                    content = $"请根据以下关键词生成完整的剧本杀广告" +
                    $"严格按照以下格式生成内容：" +
                    $"剧本背景: ...\n玩家目标: ...\n核心创意: ...\n" +
                    $"每次生成的内容必须独特，可以通过改变背景设定、角色类型、目标描述或核心创意的表达方式来实现。" +
                    $"关键词包括以下几点：{request.Prompt}" }
                    },
                    model = "gpt-3.5-turbo",
                    temperature = 1.0, // 提高随机性
                    top_p = 0.9,       // 增加多样性
                    max_tokens = 600,
                    n = 1, // 流式响应每次只处理一个结果
                    stream = true // 启用流式输出
                };

                var requestContent = new StringContent(
                    JsonConvert.SerializeObject(requestData, _jsonSettings),
                    Encoding.UTF8,
                    "application/json");

                // 创建HTTP请求但不立即等待响应
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions");
                httpRequest.Content = requestContent;

                var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                // 获取响应流
                var stream = await response.Content.ReadAsStreamAsync();
                reader = new StreamReader(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"流式生成标语失败: {ex.Message}");
                yield break;
            }

            // 不在try-catch块内使用yield
            if (reader != null)
            {
                // 将流处理逻辑移到try-catch外部
                await foreach (var slogan in ProcessStreamAsync(reader))
                {
                    yield return slogan;
                }
            }
        }


        // 单独的流处理方法
        private async IAsyncEnumerable<Slogan> ProcessStreamAsync(StreamReader reader)
        {
            // 用于累积内容
            var contentBuilder = new StringBuilder();
            string line = null;

            // 读取流式响应
            while (!reader.EndOfStream)
            {
                try
                {
                    line = await reader.ReadLineAsync();
                }
                catch (Exception)
                {
                    break; // 在读取时出现错误，退出循环
                }

                if (string.IsNullOrEmpty(line)) continue;
                if (!line.StartsWith("data:")) continue;

                line = line.Substring(5).Trim();
                if (line == "[DONE]") break;

                StreamResponse chunk = null;
                try
                {
                    chunk = JsonConvert.DeserializeObject<StreamResponse>(line);
                }
                catch (Newtonsoft.Json.JsonException)
                {
                    continue; // 忽略JSON解析错误
                }

                if (chunk?.Choices == null || chunk.Choices.Count == 0) continue;

                var contentDelta = chunk.Choices[0]?.Delta?.Content;
                if (string.IsNullOrEmpty(contentDelta)) continue;

                contentBuilder.Append(contentDelta);

                // 检查是否有完整的部分可以输出（例如完成了一个段落）
                if (contentDelta.Contains("\n\n") || contentDelta.EndsWith(".") || contentDelta.EndsWith("。"))
                {
                    var content = contentBuilder.ToString();
                    var coreIdea = ExtractCoreIdea(content);

                    // 如果已经有核心创意，可以构建和返回一个Slogan对象
                    if (!string.IsNullOrWhiteSpace(coreIdea))
                    {
                        // 使用正则表达式移除 "核心创意" 部分及其后续内容
                        var cleanContent = Regex.Replace(content, @"\n\n核心创意[:：].+", string.Empty, RegexOptions.Singleline).Trim();

                        yield return new Slogan
                        {
                            Content = cleanContent,
                            CoreIdea = coreIdea
                        };

                        // 重置内容构建器
                        contentBuilder.Clear();
                    }
                }
            }

            // 处理剩余内容
            var remainingContent = contentBuilder.ToString().Trim();
            if (!string.IsNullOrEmpty(remainingContent))
            {
                var coreIdea = ExtractCoreIdea(remainingContent);

                // 使用正则表达式移除 "核心创意" 部分及其后续内容
                if (!string.IsNullOrWhiteSpace(coreIdea))
                {
                    remainingContent = Regex.Replace(remainingContent, @"\n\n核心创意[:：].+", string.Empty, RegexOptions.Singleline).Trim();
                }

                yield return new Slogan
                {
                    Content = remainingContent,
                    CoreIdea = coreIdea ?? string.Empty
                };
            }
        }



        private class StreamResponse
        {
            public List<StreamChoice> Choices { get; set; }

            public class StreamChoice
            {
                public StreamDelta Delta { get; set; }
            }

            public class StreamDelta
            {
                public string Content { get; set; }
            }
        }


        public async Task<Detail> GenFramework(List<Message> msgs)
        {
            msgs.Insert(0, new Message { Role = "system", Content = systemmsg });
            string final = await GetApiOutput(msgs);
            return System.Text.Json.JsonSerializer.Deserialize<Detail>(final);
        }

        public async Task<String> GenDetail(string frame, string title)//生成剧本细节
        {
            //从剧本获取人物剧本
            string pattern = "人物剧本\n\n([\\s\\S]*?)\n---\n\n## 线索\n\n";
            Match match = Regex.Match(frame, pattern);
            string tempmatch = match.Value;
            tempmatch = tempmatch.Substring(12, tempmatch.Length - 25);
            string[] temps = tempmatch.Split("- CHR ");
            List<string> chrScript = new List<string>(temps);

            List<List<Message>> msgs = new List<List<Message>>();
            string finalout = "# " + title + "\n\n---\n\n## 背景\n";


            for (int i = 0; i < 5; i += 1)
            {
                List<Message> temp = new List<Message>();
                if (i == 1) { continue; }
                temp.Add(new Message { Role = "system", Content = limit[i] });
                temp.Add(new Message { Role = "user", Content = frame + "\n\n" + command[i] });//框架+完善prompt
                msgs.Add(temp);
            }
            for (int j = 0; j < chrScript.Count; j += 1)
            {
                List<Message> temp = new List<Message>();
                temp.Add(new Message { Role = "system", Content = limit[1] });
                temp.Add(new Message { Role = "user", Content = frame + "\n\n" + command[1] + "\n\n" + chrScript[j] });
                msgs.Add(temp);
            }

            List<Task<string>> tasks = new List<Task<string>>();
            for (int i = 0; i < msgs.Count; i += 1)
            {
                tasks.Add(GetApiOutput(msgs[i]));
            }

            string[] result;
            try
            {
                result = await Task.WhenAll(tasks);//并行调用
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API请求失败: {ex.Message}  ,  {ex.HelpLink}");
                return "null";
            }

            finalout = finalout + result[0] + "\n\n---\n\n## 人物剧本\n\n---\n\n";
            for (int i = 4; i < msgs.Count; i += 1)
            {
                finalout += "### " + result[i] + "\n\n---\n\n";
            }
            finalout = finalout + "## 线索\n\n" + result[1] + "\n\n---\n\n## 真相\n\n" + result[2] + "\n---\n\n## 组织者手册\n\n" + result[3];
            return finalout;
        }

        public async IAsyncEnumerable<string> ChatStreamAsync(ChatRequestEntity request)
        {
            var messages = new List<object>();

            // 添加 system 提示
            messages.Add(new { role = "system", content = "你是一个剧本杀AI助手，请用言简意赅中文回答用户问题。" });

            // 组装历史对话
            if (request.History != null)
            {
                for (int i = 0; i < request.History.Count; i++)
                {
                    messages.Add(new { role = i % 2 == 0 ? "user" : "assistant", content = request.History[i] });
                }
            }

            // 当前用户输入
            messages.Add(new { role = "user", content = request.Message });

            StreamReader reader = null;
            try
            {
                var requestData = new
                {
                    messages = messages,
                    model = "gpt-3.5-turbo",
                    temperature = 1.0,
                    top_p = 0.9,
                    max_tokens = 1024,
                    stream = true
                };

                var requestContent = new StringContent(
                    JsonConvert.SerializeObject(requestData, _jsonSettings),
                    Encoding.UTF8,
                    "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions");
                httpRequest.Content = requestContent;

                var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                reader = new StreamReader(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"流式对话失败: {ex.Message}");
                yield break;
            }

            if (reader != null)
            {
                string errorLine = null;

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line) || !line.StartsWith("data:")) continue;
                    line = line.Substring(5).Trim();
                    if (line == "[DONE]") break;

                    dynamic chunk;
                    try
                    {
                        chunk = JsonConvert.DeserializeObject(line);
                    }
                    catch
                    {
                        // 只记录 error，不 yield
                        if (line.Contains("\"error\""))
                        {
                            errorLine = line;
                        }
                        continue;
                    }

                    if (chunk?.choices == null || chunk.choices.Count == 0)
                        continue;

                    var contentDelta = chunk.choices[0]?.delta?.content?.ToString();
                    if (string.IsNullOrEmpty(contentDelta)) continue;

                    yield return contentDelta;
                }

                // 循环结束后统一 yield error
                if (!string.IsNullOrEmpty(errorLine))
                {
                    yield return errorLine;
                }
            }
        }


        public async Task<string> GetApiOutput(List<Message> msgs)//调用AI功能
        {
            var requestBody = new
            {
                model = "deepseek-reasoner",
                messages = msgs.Select(m => new { role = m.Role, content = m.Content }),
                max_tokens = 8192
            };
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var json = System.Text.Json.JsonSerializer.Serialize(requestBody, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync($"{ApiBaseUrl}chat/completions", content);
            response.EnsureSuccessStatusCode();
            string responseJson = await response.Content.ReadAsStringAsync();
            DeepSeekResponse result = System.Text.Json.JsonSerializer.Deserialize<DeepSeekResponse>(responseJson);

            string fintemp = result?.Choices[0].Mes.Content ?? "No response";

            fintemp = fintemp.Trim('`');//数据格式化
            fintemp = fintemp.TrimStart("json".ToCharArray()).Trim();
            return fintemp;
        }
        public async Task<string> GetV3Output(List<Message> msgs)
        {
            var requestBody = new
            {
                model = "deepseek-chat",
                messages = msgs.Select(m => new { role = m.Role, content = m.Content }),
                max_tokens = 8192,
                response_format = new { type = "json_object" }
            };
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var json = System.Text.Json.JsonSerializer.Serialize(requestBody, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync($"{ApiBaseUrl}chat/completions", content);
            response.EnsureSuccessStatusCode();
            string responseJson = await response.Content.ReadAsStringAsync();
            DeepSeekResponse result = System.Text.Json.JsonSerializer.Deserialize<DeepSeekResponse>(responseJson);
            string fintemp = result?.Choices[0].Mes.Content ?? "No response";
            fintemp = fintemp.Trim('`');//数据格式化
            fintemp = fintemp.TrimStart("json".ToCharArray()).Trim();
            return fintemp;
        }

        private class ApiResponse
        {
            public List<Choice> Choices { get; set; }

            public class Choice
            {
                public Message Message { get; set; }
            }

            public class Message
            {
                public string Content { get; set; }
            }
        }
        public async Task<string> AnalyzeScriptContent(string scriptContent)
        {
            var msgs = new List<Message>
            {
                new Message { Role = "system", Content = analyzePrompt },
                new Message { Role = "user", Content = scriptContent }
            };

            string result = await GetApiOutput(msgs);

            var finalResult = JsonConvert.DeserializeObject<AIAnalysisResult>(result);

            return ConvertToMarkdown(finalResult);
        }
        public string ConvertToMarkdown(AIAnalysisResult result)
        {
            if (result?.Analysis == null)
                return "无分析结果";

            var analysis = result.Analysis;
            var markdown = new StringBuilder();

            // 1. 添加标题
            markdown.AppendLine("## 游戏脚本分析报告");
            markdown.AppendLine();

            // 2. 亮点部分
            markdown.AppendLine("### ✨ 亮点");
            if (analysis.Point?.Count > 0)
            {
                foreach (var point in analysis.Point)
                    markdown.AppendLine($"- {point}");
            }
            else
                markdown.AppendLine("- 无特别亮点");
            markdown.AppendLine();

            // 3. 难点部分
            markdown.AppendLine("### ⚠️ 难点");
            if (analysis.Difficulty?.Count > 0)
            {
                foreach (var difficulty in analysis.Difficulty)
                    markdown.AppendLine($"- {difficulty}");
            }
            else
                markdown.AppendLine("- 无明显难点");
            markdown.AppendLine();

            // 4. 改进建议
            markdown.AppendLine("### 💡 改进建议");
            if (analysis.Suggestion?.Count > 0)
            {
                foreach (var suggestion in analysis.Suggestion)
                    markdown.AppendLine($"- {suggestion}");
            }
            else
                markdown.AppendLine("- 无改进建议");
            markdown.AppendLine();

            // 5. 评分部分（表格形式）
            markdown.AppendLine("### ⭐ 综合评分");
            markdown.AppendLine("| 维度 | 评分（满分100） |");
            markdown.AppendLine("|------|--------------|");
            markdown.AppendLine($"| 逻辑性 | {analysis.Score?.Logicality ?? 0} |");
            markdown.AppendLine($"| 故事性 | {analysis.Score?.Storiness ?? 0} |");
            markdown.AppendLine($"| 体验感 | {analysis.Score?.Experience ?? 0} |");
            markdown.AppendLine();

            // 6. 添加总结
            var avgScore = (analysis.Score?.Logicality + analysis.Score?.Storiness + analysis.Score?.Experience) / 3.0;
            markdown.AppendLine($"**综合平均分：{avgScore:0.0}/100**");

            return markdown.ToString();
        }


        public async Task<ImageMsgs> GenDescription(string final)  //获取特征
        {
            List<Message> chrmsgs = new List<Message>();
            chrmsgs.Add(new Message { Role = "system", Content = descChr });
            chrmsgs.Add(new Message { Role = "user", Content = final + "\n\n" + "请帮我描述并补充以上剧本中的人物外貌，并以json的格式输出" });
            List<Message> scmsgs = new List<Message>();
            scmsgs.Add(new Message { Role = "system", Content = descscene });
            scmsgs.Add(new Message { Role = "user", Content = final + "\n\n" + "请帮我提取并补充以上剧本中的场景描绘，并以json的格式输出" });
            List<Message> propmsgs = new List<Message>();
            propmsgs.Add(new Message { Role = "system", Content = descprop });
            propmsgs.Add(new Message { Role = "user", Content = final + "\n\n" + "请帮我提取并补充以上剧本对关键道具的外观描写，并以json的格式输出" });
            List<Task<string>> tasks = new List<Task<string>>();
            tasks.Add(GetV3Output(chrmsgs));
            tasks.Add(GetV3Output(scmsgs));
            tasks.Add(GetV3Output(propmsgs));
            ThingDesc chrdesc, scenedesc, propdesc;
            try
            {
                string[] descriptions = await Task.WhenAll(tasks);
                chrdesc = System.Text.Json.JsonSerializer.Deserialize<ThingDesc>(descriptions[0]);
                scenedesc = System.Text.Json.JsonSerializer.Deserialize<ThingDesc>(descriptions[1]);
                propdesc = System.Text.Json.JsonSerializer.Deserialize<ThingDesc>(descriptions[2]);
                List<ThingDesc> AllDesc = new List<ThingDesc>();
                AllDesc.Add(chrdesc);
                AllDesc.Add(scenedesc);
                AllDesc.Add(propdesc);
                return new ImageMsgs { thingDesc = AllDesc };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API请求失败: {ex.Message}");
                return null;
            }

        }

        public async Task<string> GenerateImageUrl(string type, string name, string imageDesc)
        {
            string strtemp;
            switch (type)
            {
                case "Character":
                    strtemp = await GenerateImagesAsync(imageDesc + "  写实主义");
                    if (string.IsNullOrEmpty(strtemp))
                    {
                        return null;
                    }
                    break;
                case "Scene":
                    strtemp = await GenerateImagesAsync(name + "：" + imageDesc, "不要生成人物、人、人类、男人、女人、小孩、脸、行人");
                    if (string.IsNullOrEmpty(strtemp))
                    {
                        return null;
                    }
                    break;
                default:
                    strtemp = await GenerateImagesAsync(name + "：" + imageDesc, "不要生成人物、人、人类、男人、女人、小孩、脸、行人");
                    if (string.IsNullOrEmpty(strtemp))
                    {
                        return null;
                    }
                    break;
            }
            return strtemp;
        }

        //word to picture
        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
        public async Task<string> GenerateImagesAsync(  //生成图片
            string prompt,
            string negativePrompt = null,
            string size = "1024*1024",
            int n = 1)
        {
            ImageParameters parameters = new ImageParameters
            {
                size = size,
                n = Clamp(n, 1, 4),
                prompt_extend = true,
                watermark = false
            };
            try
            {
                string taskId = await SubmitImageGenerationTask(prompt, negativePrompt, parameters);
                List<string> imageUrls = await WaitForTaskCompletion(taskId);
                return imageUrls[0];
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API请求失败: {ex.Message} ,  {ex.HelpLink}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        private async Task<string> SubmitImageGenerationTask(string prompt, string negativePrompt, ImageParameters p)  //提交生成图片任务
        {
            string apiUrl = $"{Picture_Url}/api/v1/services/aigc/text2image/image-synthesis";

            var requestData = new
            {
                model = "wanx2.1-t2i-turbo",
                input = new
                {
                    prompt = prompt,
                    negative_prompt = string.IsNullOrEmpty(negativePrompt) ? null : negativePrompt
                },
                parameters = p
            };

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {PICTURE_API_KEY}");
            client.DefaultRequestHeaders.Add("X-DashScope-Async", "enable");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();

            JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("output", out var output) &&
                output.TryGetProperty("task_id", out var taskId))
            {
                return taskId.GetString();
            }
            throw new Exception("无法从响应中获取任务ID");
        }
        private async Task<List<string>> WaitForTaskCompletion(string taskId, int maxRetries = 20, int delaySeconds = 5) //等待图片生成
        {
            string apiUrl = $"{Picture_Url}/api/v1/tasks/{taskId}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {PICTURE_API_KEY}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            for (int i = 0; i < maxRetries; i++)
            {
                Console.Write(".");
                var response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("output", out var output))
                {
                    string status = output.GetProperty("task_status").GetString();

                    if (status == "SUCCEEDED")
                    {
                        var imageUrls = new List<string>();
                        if (output.TryGetProperty("results", out var results))
                        {
                            foreach (var result in results.EnumerateArray())
                            {
                                if (result.TryGetProperty("url", out var url))
                                {
                                    imageUrls.Add(url.GetString());
                                }
                            }
                        }
                        return imageUrls.Count > 0 ? imageUrls : throw new Exception("任务成功但未获取到任何图片URL");
                    }
                    else if (status == "FAILED")
                    {
                        string code = output.GetProperty("code").GetString();
                        string message = output.GetProperty("message").GetString();
                        throw new Exception($"任务处理失败\ncode: {code}\nmessage: {message}");
                    }
                }
                await Task.Delay(delaySeconds * 1000);
            }
            throw new Exception("任务处理超时");
        }
        //word to picture end

        string[] limit = {
    "输出内容禁止使用markdown格式，仅输出背景相关内容，禁止输出其它无关文字，内容必须非常详细",
    "输出内容禁止使用markdown格式，仅输出用户提及的角色内容，禁止输出其它无关文字，内容必须非常详细",
    "输出内容禁止使用markdown格式，仅输出线索的内容，禁止输出其它无关文字，内容必须非常详细，只能在原本线索的基础上添加描述性信息以及有助于推理的要素，严禁添加线索数量"   ,
    "输出内容禁止使用markdown格式，仅输出真相内容，禁止输出其它无关文字。必须包含案件发生的完整过程、人物的目的及详细原因。",
    "输出内容禁止使用markdown格式，仅输出组织者手册相关内容，禁止输出其它无关文字，内容必须非常详细" };
        string[] command = {
        "\n\n请不遗余力地详细描述上述剧本中的 ## 背景 部分",
        "\n\n请根据上述剧本内容不遗余力地详细描述下述人物的剧本",
        "\n\n请不遗余力地详细描述上述剧本中的 ## 线索 部分",
        "\n\n请不遗余力地详细补充上述剧本中的 ## 真相 部分",
        "\n\n请不遗余力地详细描述上述剧本中的 ## 组织者手册 部分"
    };
        string systemmsg =
@"你是一名逻辑严谨的剧本杀作家，必须严格遵循以下规则：
1. 必须严格按以下模板输出,不要包含额外文本！:
    {
    ""作为作家的礼貌性回答"":""..."",
    ""标题"":""..."", 
    ""背景"":""..."",
    ""人物剧本"": [""..."",""..."",...],
    ""线索"":[""..."",""..."",...],
    ""真相"":""..."",
    ""组织者手册"": ""...""
    }

2.若有玩家人物是凶手，则需要在其剧本中写明身份；若未写明身份，需在真相处表明原因（如失忆、人格扭曲...）

3. 当用户要求细化某部分时：
   - 保持原有内容的基础上更新指定部分
   - 未修改的部分需保留原有内容
   - 始终维持完整JSON结构

4. 格式要求：
   - 严格遵循JSON语法
   - 每次响应必须完整包含所有JSON字段（作为作家的礼貌性回答、标题、背景、人物剧本、线索、真相、组织者手册），不可遗漏
   - 禁止输出\n或者\\n

5. 请按以下内容深化剧本内容：
    （1）背景：
        需包含必要的事件背景、人物来历
        事件的叙述需按时间线描述，要求详细具体
        必须提供细节描写，以帮助案件推理
    （2）人物剧本：每个角色必须包含：
        价值观在其生活中的体现
        人物背景描述
        动机（角色在本次事件中的目标）
        时间线（案发前后的行动轨迹）
        信息差（角色间掌握的信息不尽相同）
        与其他角色的关系（可以是表面也可以有不为人知的一面）
    （3）线索：
        关键性线索必须有明确的指向
        每条线索需要有足够多的细节
    （4）真相：
        必须包含凶手作案过程
        详细描述每个细节

6. 针对上述规则的示例：
    违规示例（1）：
生成人物剧本部分时，返回
""人物剧本"": {
    ""价值观"": ""..."",
    ""背景"": ""..."",
    ""角色"": ""..."",
    ""动机"": ""..."",
    ""时间线"": [
    ""19:00 ..."",
    ""20:15 ..."",
    ""21:00 ...""],
    ""信息差"": ""..."",
    ""关系"": ""...""},
    正确示例（1）：
""人物剧本"":""甲是一名...，甲...（体现价值观），甲的出身...，甲是为了...来到这里，19：00 甲做了...；20：15 甲在...；21：00 甲...；...。甲知道...的事。甲和...有...的关系，和...有...的关系...""

    违规示例（2）：
用户要求细化手册时，若仅返回{
  ""组织者手册"": ""...""
} → 视为违规
    正确示例（2）：
用户要求细化手册时，应返回完整结构{
  ""作为作家的礼貌性回答"": ""好的，已细化手册部分，其他部分保持现有内容..."",
  ""标题"": ""..."",
  ...
  ""组织者手册"": ""新细化内容...""
}

    违规示例（3）：
生成组织者手册部分，若返回错误结构
""组织者手册"":[
    ""..."",
    ""..."",
    ""..."",
    ...]  →  视为违规
    正确示例（3）：
""组织者手册"":""...""

    违规示例（4）：
生成作为作家的礼貌性回答部分，若回答
...，以下是...的完整剧本
 →  视为违规
    正确示例（4）：
...，左侧是...的完整剧本

    违规示例（5）：
用户要求细化线索时，若其它部分未返回原有部分：
 {""作为作家的礼貌性回答"": ""..."",
  ""标题"": ""..."",
  ""背景"": ""维持原有详细内容"",
  ""人物剧本"": ""维持原有详细内容"",
  ""线索"": ""..."",
  ""组织者手册"": ""维持原有详细内容""
} → 视为违规
    正确示例（5）：
用户要求细化线索时，应返回完整内容，且不替换原有部分为“维持原有详细内容”
{""作为作家的礼貌性回答"": ""..."",
  ""标题"": ""..."",
  ""背景"": ""...（原有内容）"",
  ""人物剧本"": ""...（原有内容）"",
  ""线索"": ""..."",
  ...}

    违规示例（6）：
输出“维持原有详细内容”及其相近意义的语段视为违规
}

    违规示例（7）：
生成内容时违反JSON语法规则：
 {""作为作家的礼貌性回答"": ""..."",
  ""标题"": ""..."",
  ""背景"": ""...""电梯""...于12:13坠落{一些解释}...\n死者..."",
    ....
} → 视为违规 （解释：共有4处错误，1、""电梯""使用了""而不是“，2、12:13使用了:而不是：，3、{一些解释}使用了{}而不是（）,4、\n死者中使用的\n非法，应改为。）
    正确示例（7）：
生成内容应符合JSON语法：
 {""作为作家的礼貌性回答"": ""..."",
  ""标题"": ""..."",
  ""背景"": ""...“电梯”...于12：13坠落（一些解释）...。死者..."",
    ....
}


现在请根据用户请求进行处理：";

        string descChr =
@"你擅长于提取剧本中人物的外貌描写。请遵循以下规则：
1、严格使用JSON格式，包含名称、描绘两个字段，描绘需包含人物给人的整体感受。

2、若原文有明确人物外貌描写，提取人物外貌特征到描绘中；若原文描绘不完整可适当补充。

3、若原文无人物外貌描写，则生成符合人物身份（年龄/职业/性格）的合理特征到描绘中

5、若输出与外貌描绘无关的信息，视为违规（例如输出人物关系、背景、原因等信息，是违规的）

4、必须严格按以下模板输出,不要包含额外文本！：
    {
    ""名称"":[""..."",""..."",""..."",...]
    ""描绘"":[
        ""..."",
        ""..."",
        ""..."",
        ...
    ]}

5、输出示例：
    {
    ""名称"":[""小明"",""小红"",""小王"",...]
    ""描绘"":[
        ""小明的外貌描绘..."",
        ""小红的外貌描绘..."",
        ""小王的外貌描绘..."",
        ...
    ]}

请输出JSON：";

        string descscene =
            @"你擅长于提取剧本中场景的环境描写。请遵循以下规则：
1、严格使用JSON格式，包含名称、描绘两个字段。

2、若原文有明确场景描写，则提取场景特征到描绘中。

3、若输出与场景描绘无关的信息，则视为违规

4、场景描绘中不能出现人物名称以及人物动作等与人物相关的描述

5、输出的场景 名称 和场景 描绘 必须一一对应

6、必须严格按以下模板输出,不要包含额外文本！：
    {
    ""名称"":[""..."",""..."",""..."",...]
    ""描绘"":[
        ""..."",
        ""..."",
        ""..."",
        ...
    ]}

7、输出示例：
    {
    ""名称"":[""浴室"",""悬崖"",""厨房"",...]
    ""描绘"":[
        ""浴室的场景描绘..."",
        ""悬崖的场景描绘..."",
        ""厨房的场景描绘..."",
        ...
    ]}

8、错误示例（1）：
        {
    ""名称"":[""浴室"",""悬崖"",""厨房""]
    ""描绘"":[
        ""浴室的场景描绘..."",
        ""悬崖的场景描绘..."",
    ]}
    解释：名称与描绘必须一一对应，示例中输出3个名称却只有2个描绘
    
    错误示例（2）：
        {
    ""名称"":[""浴室"",""悬崖"",""厨房""]
    ""描绘"":[
        ""浴室的场景描绘..."",
        ""悬崖连接着...李萌依靠在栏杆上..."",
        ""厨房的场景描绘...""
    ]}
    解释：悬崖的场景描绘出现人物李萌（或者描述成玩家、众人等对人的抽象），这是违规的

请输出JSON：";

        string descprop =
    @"你擅长于提取剧本中关键道具的静态外观描写。请遵循以下规则：

1、若原文有关键道具的明确的状物描写，则将其提取到描绘中；若无原文描写，则基于物品类型进行专业级外观推理

2、描绘字段只能包含物品的视觉特征，例如（不需要全部包含）：
   - 颜色、形状、材质、尺寸比例
   - 表面纹理/装饰/特殊标记
   - 磨损/氧化/使用痕迹
   - 光影反射特征

3、绝对禁止出现：
   - 物品用途或功能描述
   - 背景故事或象征意义
   - 与人物相关的任何信息
   - 非视觉特征（如气味、声音）

4、名称与描绘必须一一对应且数量相等

5、必须严格按以下模板输出,不要包含额外文本！：
    {
    ""名称"":[""..."",""..."",""..."",...]
    ""描绘"":[
        ""..."",
        ""..."",
        ""..."",
        ...
    ]}

6、输出示例：
    {
    ""名称"":[""皮医生的药箱"",""陈夫人的项链"",""王大爷的皮带"",...]
    ""描绘"":[
        ""方方正正，约莫一尺来长，半尺宽..."",
        ""链身极细，每一环都打磨得溜光水滑，在灯下泛着冷冽的银光..."",
        ""褐色的牛皮表面布满细密的纹路，带身约莫三指宽，边缘处已经被磨得发亮..."",
        ...
    ]}

7、错误示例（1）：
    {
    ""名称"":[""皮医生的药箱"",""陈夫人的项链"",""王大爷的皮带""]
    ""描绘"":[
        ""方方正正，约莫一尺来长，半尺宽..."",
        ""链身极细，每一环都打磨得溜光水滑，在灯下泛着冷冽的银光..."",
    ]}
    解释：名称与描绘必须一一对应，示例中输出3个名称却只有2个描绘

错误示例（2）：
        {
    ""名称"":[""DNA检测报告""...]
    ""描绘"":[
        ""显示你与陈天明存在生物学父女关系"",
        ...
    ]}
    解释：输出内容涉及人物，不是纯粹的外观描写

请输出JSON：";

        string analyzePrompt = @"
你是一名剧本杀质量评估员，请遵循以下规则对用户给与的剧本杀进行评估：
    1、按指定JSON格式输出以下字段：
        亮点（数组）
        难点（数组）
        改进建议（数组）
        综合评分（包含逻辑性、故事性、体验感三项整数评分）

    输出需符合如下结构：
    {
        ""analysis"": {
        ""point"": [""...""],
        ""difficulty"": [""...""],
        ""suggestion"": [""...""],
        ""score"": {
            ""logicality"": 0,
            ""storiness"": 0,
            ""experience"": 0
                }
        }
    
    2、评分部分满分为100
        logicality为对剧本故事、人物行为的逻辑性评分
        storiness为对剧本故事性的评分
        experience是对玩家体验性的预测评分

    3、亮点、难点、改进意见的表达需要简单明了，足够详尽";
        //太长了所以放下面了
    }
}