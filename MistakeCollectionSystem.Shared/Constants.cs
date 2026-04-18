using System;
using System.Collections.Generic;
using System.Text;

namespace MistakeCollectionSystem.Shared
{
    /// <summary>
    /// 系统常量定义类
    /// 集中管理系统中使用的所有常量，便于维护和修改
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// API 相关常量
        /// </summary>
        public static class Api
        {
            /// <summary>
            /// API 版本号
            /// </summary>
            public const string Version = "v1";

            /// <summary>
            /// API 路由前缀
            /// </summary>
            public const string RoutePrefix = $"api/{Version}";

            /// <summary>
            /// CORS 策略名称
            /// </summary>
            public const string CorsPolicyName = "AllowSpecificOrigin";
        }

        /// <summary>
        /// AI 服务相关常量
        /// </summary>
        public static class AIService
        {
            /// <summary>
            /// 百度 OCR API 地址
            /// </summary>
            public const string BaiduOcrUrl = "https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic";

            /// <summary>
            /// 通义千问 API 地址
            /// </summary>
            public const string TongyiQianwenUrl = "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation";

            /// <summary>
            /// AI 请求超时时间（秒）
            /// </summary>
            public const int RequestTimeoutSeconds = 30;

            /// <summary>
            /// 最大重试次数
            /// </summary>
            public const int MaxRetryCount = 3;
        }

        /// <summary>
        /// 数据库相关常量
        /// </summary>
        public static class Database
        {
            /// <summary>
            /// 默认连接字符串名称
            /// </summary>
            public const string DefaultConnection = "DefaultConnection";

            /// <summary>
            /// 数据库迁移历史表名
            /// </summary>
            public const string MigrationHistoryTable = "__EFMigrationsHistory";
        }

        /// <summary>
        /// 文件存储相关常量
        /// </summary>
        public static class FileStorage
        {
            /// <summary>
            /// 允许的图片格式
            /// </summary>
            public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".webp" };

            /// <summary>
            /// 最大文件大小（5MB）
            /// </summary>
            public const int MaxFileSizeBytes = 5 * 1024 * 1024;

            /// <summary>
            /// 图片质量（1-100）
            /// </summary>
            public const int ImageQuality = 80;

            /// <summary>
            /// 缩略图最大宽度
            /// </summary>
            public const int ThumbnailMaxWidth = 300;

            /// <summary>
            /// 缩略图最大高度
            /// </summary>
            public const int ThumbnailMaxHeight = 300;
        }

        /// <summary>
        /// JWT 认证相关常量
        /// </summary>
        public static class Jwt
        {
            /// <summary>
            /// Token 有效期（小时）
            /// </summary>
            public const int ExpirationHours = 24;

            /// <summary>
            /// Refresh Token 有效期（天）
            /// </summary>
            public const int RefreshTokenExpirationDays = 7;
        }
    }
}
