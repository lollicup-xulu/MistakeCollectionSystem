using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MistakeCollectionSystem.Core.DTOs;
using MistakeCollectionSystem.Infrastructure.Services;
using MistakeCollectionSystem.Shared;

namespace MistakeCollectionSystem.API.Controllers;

/// <summary>
/// 错题管理控制器
/// 处理错题的上传、识别、查询和统计
/// </summary>
[ApiController]
[Route($"{Constants.Api.RoutePrefix}/[controller]")]
[Authorize] // 需要认证
public class MistakeController : ControllerBase
{
    private readonly IMistakeService _mistakeService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MistakeController> _logger;

    public MistakeController(
        IMistakeService mistakeService,
        IMemoryCache cache,
        ILogger<MistakeController> logger)
    {
        _mistakeService = mistakeService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// 上传图片并 AI 识别
    /// </summary>
    /// <param name="file">上传的图片文件</param>
    /// <returns>识别出的错题信息</returns>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ApiResponse<MistakeQuestionDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
    {
        try
        {
            // 验证文件
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<object>.Fail("请选择要上传的文件"));
            }

            // 验证文件类型
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Constants.FileStorage.AllowedImageExtensions.Contains(extension))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    $"不支持的文件格式，仅支持：{string.Join(", ", Constants.FileStorage.AllowedImageExtensions)}"));
            }

            // 验证文件大小
            if (file.Length > Constants.FileStorage.MaxFileSizeBytes)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    $"文件大小超过限制（最大 {Constants.FileStorage.MaxFileSizeBytes / 1024 / 1024} MB）"));
            }

            // 获取当前用户 ID（从 JWT Token 中获取）
            var userId = GetCurrentUserId();

            // 处理上传和识别
            using var stream = file.OpenReadStream();
            var result = await _mistakeService.ProcessImageAsync(userId, stream, file.FileName);

            // 清除相关缓存
            _cache.Remove($"mistake_statistics_{userId}");

            return Ok(ApiResponse<MistakeQuestionDto>.Success(result, "图片上传并识别成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传图片失败");
            return StatusCode(500, ApiResponse<object>.Fail("处理失败，请稍后重试", 500, ex.Message));
        }
    }

    /// <summary>
    /// 获取用户的错题列表
    /// </summary>
    /// <param name="subject">学科筛选（可选）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>错题列表</returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MistakeQuestionDto>>), 200)]
    public async Task<IActionResult> GetMistakeList(
        [FromQuery] string? subject = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var cacheKey = $"mistake_list_{userId}_{subject}_{page}_{pageSize}";

            // 尝试从缓存读取
            if (_cache.TryGetValue(cacheKey, out PagedResult<MistakeQuestionDto>? cachedResult))
            {
                return Ok(ApiResponse<PagedResult<MistakeQuestionDto>>.Success(cachedResult!));
            }

            // 从数据库查询
            var result = await _mistakeService.GetUserMistakesAsync(userId, subject, page, pageSize);

            // 缓存结果（5分钟）
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return Ok(ApiResponse<PagedResult<MistakeQuestionDto>>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取错题列表失败");
            return StatusCode(500, ApiResponse<object>.Fail("获取失败", 500, ex.Message));
        }
    }

    /// <summary>
    /// 获取错题统计信息（按学科、知识点）
    /// </summary>
    /// <returns>统计信息</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<MistakeStatisticsDto>), 200)]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var userId = GetCurrentUserId();
            var cacheKey = $"mistake_statistics_{userId}";

            // 尝试从缓存读取
            if (_cache.TryGetValue(cacheKey, out MistakeStatisticsDto? cachedStats))
            {
                return Ok(ApiResponse<MistakeStatisticsDto>.Success(cachedStats!));
            }

            // 计算统计信息
            var stats = await _mistakeService.GetStatisticsAsync(userId);

            // 缓存结果（10分钟）
            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));

            return Ok(ApiResponse<MistakeStatisticsDto>.Success(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取统计数据失败");
            return StatusCode(500, ApiResponse<object>.Fail("获取失败", 500, ex.Message));
        }
    }

    /// <summary>
    /// 生成错题集
    /// </summary>
    /// <param name="request">生成参数</param>
    /// <returns>错题集信息</returns>
    [HttpPost("generate-collection")]
    [ProducesResponseType(typeof(ApiResponse<MistakeCollectionDto>), 200)]
    public async Task<IActionResult> GenerateCollection([FromBody] GenerateCollectionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var collection = await _mistakeService.GenerateCollectionAsync(userId, request);

            return Ok(ApiResponse<MistakeCollectionDto>.Success(collection, "错题集生成成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成错题集失败");
            return StatusCode(500, ApiResponse<object>.Fail("生成失败", 500, ex.Message));
        }
    }

    /// <summary>
    /// 标记错题为已掌握
    /// </summary>
    /// <param name="questionId">错题 ID</param>
    /// <returns>操作结果</returns>
    [HttpPost("master/{questionId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> MarkAsMastered(int questionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _mistakeService.MarkAsMasteredAsync(userId, questionId);

            if (result)
            {
                // 清除缓存
                _cache.Remove($"mistake_list_{userId}");
                _cache.Remove($"mistake_statistics_{userId}");

                return Ok(ApiResponse<bool>.Success(true, "已标记为掌握"));
            }

            return BadRequest(ApiResponse<bool>.Fail("操作失败，请检查错题是否存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记掌握失败");
            return StatusCode(500, ApiResponse<object>.Fail("操作失败", 500, ex.Message));
        }
    }

    /// <summary>
    /// 获取当前登录用户的 ID
    /// </summary>
    private int GetCurrentUserId()
    {
        // 从 JWT Token 中获取用户 ID
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            // 开发环境返回默认用户 ID
            return 1;
        }

        return int.Parse(userIdClaim);
    }
}