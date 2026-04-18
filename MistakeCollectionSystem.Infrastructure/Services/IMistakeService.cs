using MistakeCollectionSystem.Core.DTOs;

namespace MistakeCollectionSystem.Infrastructure.Services
{
    /// <summary>
    /// 错题服务接口
    /// 定义错题相关的业务逻辑
    /// </summary>
    public interface IMistakeService
    {
        /// <summary>
        /// 处理上传的图片，进行AI识别并保存
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="imageStream">图片流</param>
        /// <param name="fileName">原始文件名</param>
        /// <returns>识别出的错题信息</returns>
        Task<MistakeQuestionDto> ProcessImageAsync(int userId, Stream imageStream, string fileName);

        /// <summary>
        /// 获取用户的错题列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="subject">学科筛选（可选）</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>分页的错题列表</returns>
        Task<PagedResult<MistakeQuestionDto>> GetUserMistakesAsync(int userId, string? subject, int page, int pageSize);

        /// <summary>
        /// 获取错题统计信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>统计信息</returns>
        Task<MistakeStatisticsDto> GetStatisticsAsync(int userId);

        /// <summary>
        /// 生成错题集
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">生成参数</param>
        /// <returns>生成的错题集</returns>
        Task<MistakeCollectionDto> GenerateCollectionAsync(int userId, GenerateCollectionRequest request);

        /// <summary>
        /// 标记错题为已掌握
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="questionId">错题ID</param>
        /// <returns>是否成功</returns>
        Task<bool> MarkAsMasteredAsync(int userId, int questionId);

        /// <summary>
        /// 记录练习结果
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="practiceRecord">练习记录</param>
        /// <returns>是否成功</returns>
        Task<bool> RecordPracticeAsync(int userId, PracticeRecordDto practiceRecord);

        /// <summary>
        /// 根据知识点搜索错题
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="knowledgePoint">知识点关键词</param>
        /// <returns>错题列表</returns>
        Task<List<MistakeQuestionDto>> SearchByKnowledgePointAsync(int userId, string knowledgePoint);

        /// <summary>
        /// 获取高频错题（按错误次数排序）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="topN">获取前N个</param>
        /// <returns>高频错题列表</returns>
        Task<List<MistakeQuestionDto>> GetHighFrequencyMistakesAsync(int userId, int topN = 10);
    }
}
