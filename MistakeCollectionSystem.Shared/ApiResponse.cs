using System;
using System.Collections.Generic;
using System.Text;

namespace MistakeCollectionSystem.Shared
{
    /// <summary>
    /// 统一 API 响应格式
    /// 用于规范所有 API 接口的返回数据格式
    /// </summary>
    /// <typeparam name="T">响应数据的类型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 响应码（200成功，400客户端错误，500服务器错误）
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 响应数据
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 错误详情（仅在开发环境返回）
        /// </summary>
        public string? ErrorDetail { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 成功响应工厂方法
        /// </summary>
        public static ApiResponse<T> Success(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Code = 200,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 失败响应工厂方法
        /// </summary>
        public static ApiResponse<T> Fail(string message, int code = 400, string? errorDetail = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Code = code,
                Message = message,
                ErrorDetail = errorDetail
            };
        }
    }
}
