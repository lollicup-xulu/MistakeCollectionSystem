using MistakeCollectionSystem.Core.DTOs;
using MistakeCollectionSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MistakeCollectionSystem.Infrastructure.Services
{
    /// <summary>
    /// 认证服务接口
    /// </summary>
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(string username, string password);
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<string> GenerateJwtTokenAsync(User user);
    }

}
