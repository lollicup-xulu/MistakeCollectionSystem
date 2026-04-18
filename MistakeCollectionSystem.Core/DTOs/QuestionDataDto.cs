using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MistakeCollectionSystem.Core.DTOs
{
    /// <summary>
    /// AI 识别结果的 DTO
    /// 用于接收 AI 服务返回的标准化数据
    /// </summary>
    public class QuestionDataDto
    {
        /// <summary>
        /// 学科
        /// </summary>
        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// 题型
        /// </summary>
        [JsonPropertyName("questionType")]
        public string QuestionType { get; set; } = string.Empty;

        /// <summary>
        /// 题目文本
        /// </summary>
        [JsonPropertyName("questionText")]
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// 正确答案
        /// </summary>
        [JsonPropertyName("correctAnswer")]
        public string CorrectAnswer { get; set; } = string.Empty;

        /// <summary>
        /// 用户的错误答案
        /// </summary>
        [JsonPropertyName("userAnswer")]
        public string UserAnswer { get; set; } = string.Empty;

        /// <summary>
        /// 知识点列表
        /// </summary>
        [JsonPropertyName("knowledgePoints")]
        public List<string> KnowledgePoints { get; set; } = new();

        /// <summary>
        /// 难度等级
        /// </summary>
        [JsonPropertyName("difficultyLevel")]
        public int DifficultyLevel { get; set; } = 3;

        /// <summary>
        /// 错误原因分析
        /// </summary>
        [JsonPropertyName("mistakeReasonAnalysis")]
        public string MistakeReasonAnalysis { get; set; } = string.Empty;
    }
}
