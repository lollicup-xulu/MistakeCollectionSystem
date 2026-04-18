using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MistakeCollectionSystem.Core.DTOs;
using MistakeCollectionSystem.Core.Entities;
using MistakeCollectionSystem.Infrastructure.Data;
using MistakeCollectionSystem.Infrastructure.Services.AI;
using MistakeCollectionSystem.Shared;
using System.Text.Json;

namespace MistakeCollectionSystem.Infrastructure.Services
{
    /// <summary>
    /// 错题服务实现类
    /// 处理错题的核心业务逻辑
    /// </summary>
    public class MistakeService : IMistakeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIParser _aiParser;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<MistakeService> _logger;

        public MistakeService(
            ApplicationDbContext context,
            IAIParser aiParser,
            IFileStorageService fileStorage,
            ILogger<MistakeService> logger)
        {
            _context = context;
            _aiParser = aiParser;
            _fileStorage = fileStorage;
            _logger = logger;
        }

        /// <summary>
        /// 处理上传的图片
        /// </summary>
        public async Task<MistakeQuestionDto> ProcessImageAsync(int userId, Stream imageStream, string fileName)
        {
            // 1. 保存原始图片
            var imageUrl = await _fileStorage.SaveImageAsync(imageStream, fileName, userId);
            var thumbnailUrl = await _fileStorage.CreateThumbnailAsync(imageStream, fileName, userId);

            // 2. 创建原始记录
            var rawRecord = new MistakeRawRecord
            {
                UserId = userId,
                OriginalImageUrl = imageUrl,
                ThumbnailUrl = thumbnailUrl,
                AIProcessStatus = 1, // 处理中
                CreatedAt = DateTime.UtcNow
            };

            _context.MistakeRawRecords.Add(rawRecord);
            await _context.SaveChangesAsync();

            try
            {
                // 3. AI识别
                var questionData = await _aiParser.ParseImageToQuestionAsync(imageStream);

                // 保存AI原始响应
                rawRecord.AIResponseJson = JsonSerializer.Serialize(questionData);
                rawRecord.AIProcessStatus = 2; // 成功
                rawRecord.ProcessedAt = DateTime.UtcNow;

                // 4. 检查是否存在相似错题（避免重复）
                var existingQuestion = await FindSimilarQuestionAsync(userId, questionData);

                MistakeQuestion question;
                if (existingQuestion != null)
                {
                    // 增加错题次数
                    existingQuestion.IncrementMistakeCount();
                    existingQuestion.UpdatedAt = DateTime.UtcNow;
                    question = existingQuestion;
                    _context.MistakeQuestions.Update(existingQuestion);
                }
                else
                {
                    // 创建新错题
                    question = new MistakeQuestion
                    {
                        UserId = userId,
                        RawRecordId = rawRecord.Id,
                        Subject = questionData.Subject,
                        QuestionType = questionData.QuestionType,
                        QuestionText = questionData.QuestionText,
                        CorrectAnswer = questionData.CorrectAnswer,
                        UserAnswer = questionData.UserAnswer,
                        MistakeReason = questionData.MistakeReasonAnalysis,
                        KnowledgePoints = string.Join(",", questionData.KnowledgePoints),
                        DifficultyLevel = questionData.DifficultyLevel,
                        MistakeCount = 1,
                        LastMistakeAt = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.MistakeQuestions.Add(question);
                }

                await _context.SaveChangesAsync();

                // 5. 返回DTO
                return await MapToDtoAsync(question);
            }
            catch (Exception ex)
            {
                // 更新原始记录状态为失败
                rawRecord.AIProcessStatus = 3; // 失败
                rawRecord.ErrorMessage = ex.Message;
                rawRecord.ProcessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogError(ex, "AI识别失败，用户ID: {UserId}, 文件: {FileName}", userId, fileName);
                throw;
            }
        }

        /// <summary>
        /// 查找相似错题（基于题目文本相似度）
        /// </summary>
        private async Task<MistakeQuestion?> FindSimilarQuestionAsync(int userId, QuestionDataDto questionData)
        {
            // 简化版：比较题目文本的前100个字符
            var questionTextKey = questionData.QuestionText.Length > 100
                ? questionData.QuestionText[..100]
                : questionData.QuestionText;

            return await _context.MistakeQuestions
                .Where(q => q.UserId == userId
                    && q.IsActive
                    && !q.IsMastered
                    && q.QuestionText != null
                    && q.QuestionText.StartsWith(questionTextKey))
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 获取用户的错题列表（分页）
        /// </summary>
        public async Task<PagedResult<MistakeQuestionDto>> GetUserMistakesAsync(
            int userId, string? subject, int page, int pageSize)
        {
            var query = _context.MistakeQuestions
                .Where(q => q.UserId == userId && q.IsActive && !q.IsMastered)
                .AsNoTracking();

            // 按学科筛选
            if (!string.IsNullOrEmpty(subject))
            {
                query = query.Where(q => q.Subject == subject);
            }

            // 获取总数
            var totalCount = await query.CountAsync();

            // 分页查询，按错误次数降序排序
            var questions = await query
                .OrderByDescending(q => q.MistakeCount)
                .ThenByDescending(q => q.LastMistakeAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = new List<MistakeQuestionDto>();
            foreach (var question in questions)
            {
                items.Add(await MapToDtoAsync(question));
            }

            return new PagedResult<MistakeQuestionDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        /// <summary>
        /// 获取错题统计信息
        /// </summary>
        public async Task<MistakeStatisticsDto> GetStatisticsAsync(int userId)
        {
            var questions = await _context.MistakeQuestions
                .Where(q => q.UserId == userId && q.IsActive && !q.IsMastered)
                .ToListAsync();

            // 按学科统计
            var subjectStats = questions
                .GroupBy(q => q.Subject ?? "未分类")
                .Select(g => new SubjectStatisticDto
                {
                    Subject = g.Key,
                    Count = g.Count(),
                    TotalMistakeFrequency = g.Sum(q => q.MistakeCount),
                    AverageDifficulty = g.Average(q => q.DifficultyLevel)
                })
                .ToList();

            // 按知识点统计（Top 10）
            var knowledgePointStats = questions
                .SelectMany(q => (q.KnowledgePoints ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Where(kp => !string.IsNullOrWhiteSpace(kp))
                .GroupBy(kp => kp.Trim())
                .Select(g => new KnowledgePointStatisticDto
                {
                    KnowledgePoint = g.Key,
                    Frequency = g.Count(),
                    AverageDifficulty = questions
                        .Where(q => (q.KnowledgePoints ?? "").Contains(g.Key))
                        .Average(q => q.DifficultyLevel)
                })
                .OrderByDescending(k => k.Frequency)
                .Take(10)
                .ToList();

            // 高频错题（错误次数最多的5个）
            var highFrequencyQuestions = questions
                .OrderByDescending(q => q.MistakeCount)
                .Take(5)
                .Select(q => new HighFrequencyQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText?[..Math.Min(50, q.QuestionText?.Length ?? 0)] + "...",
                    MistakeCount = q.MistakeCount,
                    Subject = q.Subject ?? "未分类"
                })
                .ToList();

            // 总体统计
            var totalStats = new TotalStatisticsDto
            {
                TotalQuestions = questions.Count,
                TotalMistakeFrequency = questions.Sum(q => q.MistakeCount),
                AverageDifficulty = questions.Any() ? questions.Average(q => q.DifficultyLevel) : 0,
                SubjectsCount = subjectStats.Count,
                MostMistakenSubject = subjectStats.OrderByDescending(s => s.TotalMistakeFrequency).FirstOrDefault()?.Subject ?? "无"
            };

            return new MistakeStatisticsDto
            {
                SubjectStatistics = subjectStats,
                KnowledgePointStatistics = knowledgePointStats,
                HighFrequencyQuestions = highFrequencyQuestions,
                TotalStatistics = totalStats,
                LastUpdated = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 生成错题集
        /// </summary>
        public async Task<MistakeCollectionDto> GenerateCollectionAsync(
            int userId, GenerateCollectionRequest request)
        {
            // 构建查询
            var query = _context.MistakeQuestions
                .Where(q => q.UserId == userId && q.IsActive && !q.IsMastered);

            // 应用筛选条件
            if (!string.IsNullOrEmpty(request.Subject))
            {
                query = query.Where(q => q.Subject == request.Subject);
            }

            if (request.KnowledgePoints != null && request.KnowledgePoints.Any())
            {
                foreach (var kp in request.KnowledgePoints)
                {
                    query = query.Where(q => q.KnowledgePoints != null && q.KnowledgePoints.Contains(kp));
                }
            }

            if (request.MinDifficulty.HasValue)
            {
                query = query.Where(q => q.DifficultyLevel >= request.MinDifficulty.Value);
            }

            if (request.MaxDifficulty.HasValue)
            {
                query = query.Where(q => q.DifficultyLevel <= request.MaxDifficulty.Value);
            }

            if (request.MinMistakeCount.HasValue)
            {
                query = query.Where(q => q.MistakeCount >= request.MinMistakeCount.Value);
            }

            // 排序
            query = request.SortBy switch
            {
                "mistakeCount" => request.SortDescending
                    ? query.OrderByDescending(q => q.MistakeCount)
                    : query.OrderBy(q => q.MistakeCount),
                "difficulty" => request.SortDescending
                    ? query.OrderByDescending(q => q.DifficultyLevel)
                    : query.OrderBy(q => q.DifficultyLevel),
                "lastMistake" => request.SortDescending
                    ? query.OrderByDescending(q => q.LastMistakeAt)
                    : query.OrderBy(q => q.LastMistakeAt),
                _ => query.OrderByDescending(q => q.MistakeCount)
            };

            // 获取题目
            var questions = await query
                .Take(request.MaxCount ?? 50)
                .ToListAsync();

            // 创建错题集
            var collection = new MistakeCollection
            {
                UserId = userId,
                CollectionName = request.Name ?? $"错题集_{DateTime.Now:yyyyMMddHHmmss}",
                GenerateType = "Auto",
                FilterConditions = JsonSerializer.Serialize(request),
                QuestionIds = JsonSerializer.Serialize(questions.Select(q => q.Id)),
                QuestionCount = questions.Count,
                IsGenerated = true,
                GeneratedAt = DateTime.UtcNow,
                GenerateToken = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow
            };

            _context.MistakeCollections.Add(collection);
            await _context.SaveChangesAsync();

            // 转换为DTO
            return new MistakeCollectionDto
            {
                Id = collection.Id,
                Name = collection.CollectionName,
                QuestionCount = collection.QuestionCount,
                Questions = questions.Select(q => MapToDtoAsync(q).Result).ToList(),
                GeneratedAt = collection.GeneratedAt ?? DateTime.UtcNow,
                ShareToken = collection.GenerateToken
            };
        }

        /// <summary>
        /// 标记错题为已掌握
        /// </summary>
        public async Task<bool> MarkAsMasteredAsync(int userId, int questionId)
        {
            var question = await _context.MistakeQuestions
                .FirstOrDefaultAsync(q => q.Id == questionId && q.UserId == userId);

            if (question == null)
            {
                return false;
            }

            question.MarkAsMastered();
            await _context.SaveChangesAsync();

            _logger.LogInformation("用户 {UserId} 标记错题 {QuestionId} 为已掌握", userId, questionId);
            return true;
        }

        /// <summary>
        /// 记录练习结果
        /// </summary>
        public async Task<bool> RecordPracticeAsync(int userId, PracticeRecordDto practiceRecord)
        {
            var question = await _context.MistakeQuestions
                .FirstOrDefaultAsync(q => q.Id == practiceRecord.QuestionId && q.UserId == userId);

            if (question == null)
            {
                return false;
            }

            var record = new PracticeRecord
            {
                QuestionId = practiceRecord.QuestionId,
                UserId = userId,
                IsCorrect = practiceRecord.IsCorrect,
                TimeSpentSeconds = practiceRecord.TimeSpentSeconds,
                PracticeDate = DateTime.UtcNow
            };

            _context.PracticeRecords.Add(record);

            // 如果连续正确3次，可以自动标记为掌握
            if (practiceRecord.IsCorrect)
            {
                var recentPractices = await _context.PracticeRecords
                    .Where(p => p.QuestionId == practiceRecord.QuestionId && p.UserId == userId)
                    .OrderByDescending(p => p.PracticeDate)
                    .Take(3)
                    .ToListAsync();

                if (recentPractices.Count >= 3 && recentPractices.All(p => p.IsCorrect))
                {
                    await MarkAsMasteredAsync(userId, practiceRecord.QuestionId);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 根据知识点搜索错题
        /// </summary>
        public async Task<List<MistakeQuestionDto>> SearchByKnowledgePointAsync(int userId, string knowledgePoint)
        {
            var questions = await _context.MistakeQuestions
                .Where(q => q.UserId == userId
                    && q.IsActive
                    && !q.IsMastered
                    && q.KnowledgePoints != null
                    && q.KnowledgePoints.Contains(knowledgePoint))
                .OrderByDescending(q => q.MistakeCount)
                .Take(20)
                .ToListAsync();

            var result = new List<MistakeQuestionDto>();
            foreach (var question in questions)
            {
                result.Add(await MapToDtoAsync(question));
            }

            return result;
        }

        /// <summary>
        /// 获取高频错题
        /// </summary>
        public async Task<List<MistakeQuestionDto>> GetHighFrequencyMistakesAsync(int userId, int topN = 10)
        {
            var questions = await _context.MistakeQuestions
                .Where(q => q.UserId == userId && q.IsActive && !q.IsMastered)
                .OrderByDescending(q => q.MistakeCount)
                .Take(topN)
                .ToListAsync();

            var result = new List<MistakeQuestionDto>();
            foreach (var question in questions)
            {
                result.Add(await MapToDtoAsync(question));
            }

            return result;
        }

        /// <summary>
        /// 映射实体到DTO
        /// </summary>
        private async Task<MistakeQuestionDto> MapToDtoAsync(MistakeQuestion question)
        {
            // 获取最近的练习记录
            var recentPractices = await _context.PracticeRecords
                .Where(p => p.QuestionId == question.Id)
                .OrderByDescending(p => p.PracticeDate)
                .Take(5)
                .ToListAsync();

            return new MistakeQuestionDto
            {
                Id = question.Id,
                Subject = question.Subject ?? "未分类",
                QuestionType = question.QuestionType ?? "未知",
                QuestionText = question.QuestionText ?? "",
                CorrectAnswer = question.CorrectAnswer ?? "",
                UserAnswer = question.UserAnswer ?? "",
                MistakeReason = question.MistakeReason ?? "",
                KnowledgePoints = (question.KnowledgePoints ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                DifficultyLevel = question.DifficultyLevel,
                MistakeCount = question.MistakeCount,
                LastMistakeAt = question.LastMistakeAt,
                IsMastered = question.IsMastered,
                RecentPracticeRecords = recentPractices.Select(p => new PracticeRecordBriefDto
                {
                    IsCorrect = p.IsCorrect,
                    PracticeDate = p.PracticeDate,
                    TimeSpentSeconds = p.TimeSpentSeconds
                }).ToList()
            };
        }
    }
}
