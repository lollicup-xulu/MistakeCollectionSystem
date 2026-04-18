using System;
using System.Collections.Generic;
using System.Text;

namespace MistakeCollectionSystem.Core.Entities
{
    /// <summary>
    /// 错题集实体
    /// 存储用户生成的错题集合
    /// </summary>
    public class MistakeCollection : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 错题集名称
        /// </summary>
        public string CollectionName { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 生成类型（Auto自动生成/Manual手动选择）
        /// </summary>
        public string GenerateType { get; set; } = "Auto";

        /// <summary>
        /// 筛选条件（JSON格式）
        /// </summary>
        public string? FilterConditions { get; set; }

        /// <summary>
        /// 包含的题目ID列表（JSON数组）
        /// </summary>
        public string? QuestionIds { get; set; }

        /// <summary>
        /// 题目数量
        /// </summary>
        public int QuestionCount { get; set; }

        /// <summary>
        /// 是否已生成
        /// </summary>
        public bool IsGenerated { get; set; }

        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTime? GeneratedAt { get; set; }

        /// <summary>
        /// 分享Token（用于公开访问）
        /// </summary>
        public string? GenerateToken { get; set; }

        /// <summary>
        /// 导航属性
        /// </summary>
        public virtual User? User { get; set; }
    }
}
