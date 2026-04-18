using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MistakeCollectionSystem.Core.Entities
{
    /// <summary>
    /// 错题原始记录实体
    /// 保存用户上传的原始图片和处理状态
    /// </summary>
    public class MistakeRawRecord : BaseEntity
    {
        /// <summary>
        /// 所属用户 ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 原始图片 URL
        /// </summary>
        [MaxLength(500)]
        public string? OriginalImageUrl { get; set; }

        /// <summary>
        /// 缩略图 URL
        /// </summary>
        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// AI 处理状态
        /// 0:待处理 1:处理中 2:成功 3:失败
        /// </summary>
        public int AIProcessStatus { get; set; } = 0;

        /// <summary>
        /// 处理完成时间
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// 错误信息（处理失败时记录）
        /// </summary>
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// OCR 原始识别文本（用于调试）
        /// </summary>
        public string? OcrRawText { get; set; }

        /// <summary>
        /// AI 返回的原始 JSON（用于调试）
        /// </summary>
        public string? AIResponseJson { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// 图片宽度
        /// </summary>
        public int? ImageWidth { get; set; }

        /// <summary>
        /// 图片高度
        /// </summary>
        public int? ImageHeight { get; set; }

        /// <summary>
        /// 导航属性：所属用户
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// 导航属性：识别出的错题（一个图片可能识别出多个题目）
        /// </summary>
        public virtual ICollection<MistakeQuestion> MistakeQuestions { get; set; } = new List<MistakeQuestion>();
    }
}
