using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MistakeCollectionSystem.Core.DTOs;
using MistakeCollectionSystem.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MistakeCollectionSystem.Infrastructure.Services.AI
{
    /// <summary>
    /// 通义千问 AI 解析器实现
    /// 使用阿里云通义千问 API 进行题目识别和格式化
    /// </summary>
    public class TongyiQianwenParser : IAIParser
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TongyiQianwenParser> _logger;
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public TongyiQianwenParser(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TongyiQianwenParser> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // 从配置读取 API 密钥
            _apiKey = _configuration["AI:TongyiQianwen:ApiKey"] ?? throw new InvalidOperationException("通义千问 API Key 未配置");
            _apiUrl = _configuration["AI:TongyiQianwen:ApiUrl"] ?? Constants.AIService.TongyiQianwenUrl;

            // 配置 HttpClient
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.Timeout = TimeSpan.FromSeconds(Constants.AIService.RequestTimeoutSeconds);
        }

        /// <summary>
        /// 从图片解析题目（完整流程）
        /// </summary>
        public async Task<QuestionDataDto> ParseImageToQuestionAsync(Stream imageStream, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("开始解析图片...");

                // 步骤1: OCR 提取文字
                var ocrText = await ExtractTextFromImageAsync(imageStream, cancellationToken);
                _logger.LogInformation("OCR 提取完成，文本长度: {Length}", ocrText.Length);

                // 步骤2: AI 格式化
                var questionData = await FormatToStandardQuestionAsync(ocrText, cancellationToken);
                _logger.LogInformation("AI 格式化完成，学科: {Subject}", questionData.Subject);

                return questionData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "图片解析失败");
                throw;
            }
        }

        /// <summary>
        /// OCR 文字提取（使用百度 OCR，因为通义千问不直接提供 OCR）
        /// </summary>
        public async Task<string> ExtractTextFromImageAsync(Stream imageStream, CancellationToken cancellationToken = default)
        {
            // 这里调用百度 OCR API
            // 简化实现，实际需要调用百度 OCR 服务
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream, cancellationToken);
            var imageBytes = memoryStream.ToArray();

            // 转换为 Base64
            var base64Image = Convert.ToBase64String(imageBytes);

            // 调用百度 OCR API（需要实际实现）
            // 这里返回模拟数据
            return await Task.FromResult("这是一道数学题：计算 2 + 2 = ? 学生回答：5 正确答案：4");
        }

        /// <summary>
        /// 使用通义千问格式化题目
        /// </summary>
        public async Task<QuestionDataDto> FormatToStandardQuestionAsync(string ocrText, CancellationToken cancellationToken = default)
        {
            var prompt = BuildPrompt(ocrText);
            var requestBody = new
            {
                model = "qwen-turbo",
                input = new
                {
                    messages = new[]
                    {
                    new
                    {
                        role = "system",
                        content = "你是一个专业的题目解析助手，擅长识别和格式化各种学科题目。"
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                }
                },
                parameters = new
                {
                    result_format = "message",
                    temperature = 0.1f  // 低温度保证结果稳定性
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 重试机制
            for (int i = 0; i < Constants.AIService.MaxRetryCount; i++)
            {
                try
                {
                    var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = ParseAIResponse(responseJson);

                    return result;
                }
                catch (Exception ex) when (i < Constants.AIService.MaxRetryCount - 1)
                {
                    _logger.LogWarning(ex, "AI 请求失败，重试 {RetryCount}/{MaxRetries}", i + 1, Constants.AIService.MaxRetryCount);
                    await Task.Delay(1000 * (i + 1), cancellationToken); // 指数退避
                }
            }

            throw new Exception("AI 请求失败，已达最大重试次数");
        }

        /// <summary>
        /// 构建 AI 提示词
        /// </summary>
        private string BuildPrompt(string ocrText)
        {
            return $@"
请分析以下OCR识别出的题目内容，将其转换为标准JSON格式。

OCR识别文本：
{ocrText}

请严格按照以下JSON格式返回，不要包含其他解释文字：
{{
    ""subject"": ""学科（数学/英语/语文/物理/化学/生物/历史/地理/政治）"",
    ""questionType"": ""题型（选择题/填空题/解答题/判断题）"",
    ""questionText"": ""完整的题目文本"",
    ""correctAnswer"": ""正确答案"",
    ""userAnswer"": ""用户给出的错误答案（如果有）"",
    ""knowledgePoints"": [""知识点1"", ""知识点2""],
    ""difficultyLevel"": 数字1-5（1最简单，5最难）,
    ""mistakeReasonAnalysis"": ""用户可能出错的原因分析""
}}

如果某些信息不明确，请根据上下文合理推断。用户答案如果未提供，请根据常见错误模式推断一个可能的错误答案。";
        }

        /// <summary>
        /// 解析 AI 返回的 JSON
        /// </summary>
        private QuestionDataDto ParseAIResponse(string responseJson)
        {
            try
            {
                using var document = JsonDocument.Parse(responseJson);
                var root = document.RootElement;

                // 通义千问返回格式解析
                var output = root.GetProperty("output");
                var choices = output.GetProperty("choices");
                var message = choices[0].GetProperty("message");
                var content = message.GetProperty("content").GetString();

                if (string.IsNullOrEmpty(content))
                    throw new InvalidOperationException("AI 返回内容为空");

                // 提取 JSON 部分（可能包含 markdown 代码块）
                var jsonContent = ExtractJson(content);
                var result = JsonSerializer.Deserialize<QuestionDataDto>(jsonContent);

                return result ?? throw new InvalidOperationException("JSON 反序列化失败");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析 AI 响应失败，原始响应: {Response}", responseJson);
                throw;
            }
        }

        /// <summary>
        /// 从文本中提取 JSON
        /// </summary>
        private string ExtractJson(string text)
        {
            // 处理 markdown 代码块
            var startIndex = text.IndexOf('{');
            var endIndex = text.LastIndexOf('}');

            if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
            {
                return text.Substring(startIndex, endIndex - startIndex + 1);
            }

            return text;
        }
    }
}
