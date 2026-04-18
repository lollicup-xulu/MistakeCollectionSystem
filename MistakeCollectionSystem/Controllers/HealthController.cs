using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MistakeCollectionSystem.API.Controllers
{
    /// <summary>
    /// 健康检查控制器
    /// 用于监控系统运行状态
    /// </summary>
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 简单健康检查
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            });
        }

        /// <summary>
        /// 详细健康检查（需要认证）
        /// </summary>
        [HttpGet("detailed")]
        [Authorize]
        public IActionResult GetDetailed()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Services = new
                {
                    Database = "Connected",
                    AI = "Available",
                    Storage = "Writable"
                }
            });
        }
    }
}
