using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MistakeCollectionSystem.Core.Entities
{
    /// <summary>
    /// 错题实体
    /// 存储 AI 识别后的标准化错题数据
    /// </summary>
    public class MistakeQuestion : BaseEntity
    {
        /// <summary>
        /// 所属用户 ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 原始记录 ID（关联上传的图片）
        /// </summary>
        public int? RawRecordId { get; set; }

        /// <summary>
        /// 学科（数学、英语、物理等）
        /// </summary>
        [MaxLength(50)]
        public string? Subject { get; set; }

        /// <summary>
        /// 题型（选择题、填空题、解答题）
        /// </summary>
        [MaxLength(20)]
        public string? QuestionType { get; set; }

        /// <summary>
        /// 题目文本（OCR 识别内容）
        /// </summary>
        public string? QuestionText { get; set; }

        /// <summary>
        /// 题目图片 URL（可选，保存截图）
        /// </summary>
        [MaxLength(500)]
        public string? QuestionImageUrl { get; set; }

        /// <summary>
        /// 正确答案
        /// </summary>
        public string? CorrectAnswer { get; set; }

        /// <summary>
        /// 用户的错误答案
        /// </summary>
        public string? UserAnswer { get; set; }

        /// <summary>
        /// 错误原因分析（AI 生成）
        /// </summary>
        [MaxLength(500)]
        public string? MistakeReason { get; set; }

        /// <summary>
        /// 知识点标签（逗号分隔）
        /// </summary>
        [MaxLength(500)]
        public string? KnowledgePoints { get; set; }

        /// <summary>
        /// 难度等级（1-5，5为最难）
        /// </summary>
        [Range(1, 5)]
        public int DifficultyLevel { get; set; } = 3;

        /// <summary>
        /// 重要程度（1-5，5为最重要）
        /// </summary>
        [Range(1, 5)]
        public int ImportanceLevel { get; set; } = 3;

        /// <summary>
        /// 错题次数（同一题目多次错误会累加）
        /// </summary>
        public int MistakeCount { get; set; } = 1;

        /// <summary>
        /// 最后一次错误时间
        /// </summary>
        public DateTime LastMistakeAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 是否仍在错题本中
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 是否已掌握
        /// </summary>
        public bool IsMastered { get; set; } = false;

        /// <summary>
        /// 掌握时间
        /// </summary>
        public DateTime? MasteredAt { get; set; }

        /// <summary>
        /// 导航属性：所属用户
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// 导航属性：原始记录
        /// </summary>
        public virtual MistakeRawRecord? RawRecord { get; set; }

        /// <summary>
        /// 导航属性：练习记录
        /// </summary>
        public virtual ICollection<PracticeRecord> PracticeRecords { get; set; } = new List<PracticeRecord>();

        /// <summary>
        /// 增加错题次数
        /// </summary>
        public void IncrementMistakeCount()
        {
            MistakeCount++;
            LastMistakeAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        /// <summary>
        /// 标记为已掌握
        /// </summary>
        public void MarkAsMastered()
        {
            IsMastered = true;
            MasteredAt = DateTime.UtcNow;
            IsActive = false; // 掌握后移出错题本
            UpdateTimestamp();
        }
    }
}
