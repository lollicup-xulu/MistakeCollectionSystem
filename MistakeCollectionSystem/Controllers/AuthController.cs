using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MistakeCollectionSystem.Core.DTOs;
using MistakeCollectionSystem.Infrastructure.Services;
using MistakeCollectionSystem.Shared;

namespace MistakeCollectionSystem.API.Controllers
{
    /// <summary>
    /// 认证控制器
    /// 处理用户登录、注册等认证相关操作
    /// </summary>
    [ApiController]
    [Route($"{Constants.Api.RoutePrefix}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request.Username, request.Password);
                return Ok(ApiResponse<LoginResponseDto>.Success(result, "登录成功"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.Fail(ex.Message, 401));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录失败: {Username}", request.Username);
                return StatusCode(500, ApiResponse<object>.Fail("登录失败，请稍后重试", 500));
            }
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(ApiResponse<RegisterResponseDto>.Success(result, "注册成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册失败: {Username}", request.Username);
                return StatusCode(500, ApiResponse<object>.Fail("注册失败，请稍后重试", 500));
            }
        }
    }
}
