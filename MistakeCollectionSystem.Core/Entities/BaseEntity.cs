using System;
using System.Collections.Generic;
using System.Text;

namespace MistakeCollectionSystem.Core.Entities
{
    /// <summary>
    /// 实体基类
    /// 所有数据库实体都继承此类，提供通用属性
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// 实体唯一标识符
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 是否已删除（软删除标志）
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// 更新时间戳
        /// </summary>
        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
