using System;
using System.Collections.Generic;
using System.Text;

namespace MistakeCollectionSystem.Core.DTOs
{
    /// <summary>
    /// 错题数据传输对象
    /// </summary>
    public class MistakeQuestionDto
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public string MistakeReason { get; set; } = string.Empty;
        public List<string> KnowledgePoints { get; set; } = new();
        public int DifficultyLevel { get; set; }
        public int MistakeCount { get; set; }
        public DateTime LastMistakeAt { get; set; }
        public bool IsMastered { get; set; }
        public List<PracticeRecordBriefDto> RecentPracticeRecords { get; set; } = new();
    }
    /// <summary>
    /// 练习记录简要DTO
    /// </summary>
    public class PracticeRecordBriefDto
    {
        public bool IsCorrect { get; set; }
        public DateTime PracticeDate { get; set; }
        public int? TimeSpentSeconds { get; set; }
    }


    /// <summary>
    /// 错题统计DTO
    /// </summary>
    public class MistakeStatisticsDto
    {
        public List<SubjectStatisticDto> SubjectStatistics { get; set; } = new();
        public List<KnowledgePointStatisticDto> KnowledgePointStatistics { get; set; } = new();
        public List<HighFrequencyQuestionDto> HighFrequencyQuestions { get; set; } = new();
        public TotalStatisticsDto TotalStatistics { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// 学科统计DTO
    /// </summary>
    public class SubjectStatisticDto
    {
        public string Subject { get; set; } = string.Empty;
        public int Count { get; set; }
        public int TotalMistakeFrequency { get; set; }
        public double AverageDifficulty { get; set; }
    }

    /// <summary>
    /// 知识点统计DTO
    /// </summary>
    public class KnowledgePointStatisticDto
    {
        public string KnowledgePoint { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public double AverageDifficulty { get; set; }
    }

    /// <summary>
    /// 高频错题DTO
    /// </summary>
    public class HighFrequencyQuestionDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int MistakeCount { get; set; }
        public string Subject { get; set; } = string.Empty;
    }

    /// <summary>
    /// 总体统计DTO
    /// </summary>
    public class TotalStatisticsDto
    {
        public int TotalQuestions { get; set; }
        public int TotalMistakeFrequency { get; set; }
        public double AverageDifficulty { get; set; }
        public int SubjectsCount { get; set; }
        public string MostMistakenSubject { get; set; } = string.Empty;
    }

    /// <summary>
    /// 生成错题集请求DTO
    /// </summary>
    public class GenerateCollectionRequest
    {
        public string? Name { get; set; }
        public string? Subject { get; set; }
        public List<string>? KnowledgePoints { get; set; }
        public int? MinDifficulty { get; set; }
        public int? MaxDifficulty { get; set; }
        public int? MinMistakeCount { get; set; }
        public int? MaxCount { get; set; }
        public string? SortBy { get; set; } = "mistakeCount";
        public bool SortDescending { get; set; } = true;
    }

    /// <summary>
    /// 错题集DTO
    /// </summary>
    public class MistakeCollectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public List<MistakeQuestionDto> Questions { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public string ShareToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// 练习记录DTO
    /// </summary>
    public class PracticeRecordDto
    {
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public int? TimeSpentSeconds { get; set; }
    }

}
