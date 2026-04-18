using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MistakeCollectionSystem.Core.Entities
{
    /// <summary>
    /// 用户实体
    /// 存储用户基本信息
    /// </summary>
    public class User : BaseEntity
    {
        /// <summary>
        /// 用户名（唯一）
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 密码哈希值（使用 BCrypt 或 PBKDF2）
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// 电子邮箱
        /// </summary>
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// 头像 URL
        /// </summary>
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// 用户角色（Student/Teacher/Admin）
        /// </summary>
        [MaxLength(20)]
        public string Role { get; set; } = "Student";

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 导航属性：用户的所有错题记录
        /// </summary>
        public virtual ICollection<MistakeQuestion> MistakeQuestions { get; set; } = new List<MistakeQuestion>();

        /// <summary>
        /// 导航属性：用户的所有练习记录
        /// </summary>
        public virtual ICollection<PracticeRecord> PracticeRecords { get; set; } = new List<PracticeRecord>();

        /// <summary>
        /// 导航属性：用户的所有错题集
        /// </summary>
        public virtual ICollection<MistakeCollection> MistakeCollections { get; set; } = new List<MistakeCollection>();
    }
}
