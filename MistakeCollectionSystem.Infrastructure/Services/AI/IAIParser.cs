using MistakeCollectionSystem.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MistakeCollectionSystem.Infrastructure.Services.AI
{
    /// <summary>
    /// AI 解析器接口
    /// 定义 AI 服务的标准方法
    /// </summary>
    public interface IAIParser
    {
        /// <summary>
        /// 从图片解析题目信息
        /// </summary>
        /// <param name="imageStream">图片流</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>解析后的题目数据</returns>
        Task<QuestionDataDto> ParseImageToQuestionAsync(Stream imageStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// 从图片中提取文字（OCR）
        /// </summary>
        /// <param name="imageStream">图片流</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>提取的文本</returns>
        Task<string> ExtractTextFromImageAsync(Stream imageStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// 使用 AI 格式化题目数据
        /// </summary>
        /// <param name="ocrText">OCR 识别的原始文本</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>标准化的题目数据</returns>
        Task<QuestionDataDto> FormatToStandardQuestionAsync(string ocrText, CancellationToken cancellationToken = default);
    }
}
