using System;
using System.Collections.Generic;
using System.Text;

namespace MistakeCollectionSystem.Core.Entities
{
    /// <summary>
    /// 练习记录实体
    /// 记录用户对错题的练习情况
    /// </summary>
    public class PracticeRecord : BaseEntity
    {
        /// <summary>
        /// 错题ID
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 是否正确
        /// </summary>
        public bool IsCorrect { get; set; }

        /// <summary>
        /// 练习时间
        /// </summary>
        public DateTime PracticeDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 耗时（秒）
        /// </summary>
        public int? TimeSpentSeconds { get; set; }

        /// <summary>
        /// 用户答案（练习时的答案，可能与原始错误答案不同）
        /// </summary>
        public string? UserAnswer { get; set; }

        /// <summary>
        /// 导航属性
        /// </summary>
        public virtual MistakeQuestion? Question { get; set; }

        public virtual User? User { get; set; }
    }
}
