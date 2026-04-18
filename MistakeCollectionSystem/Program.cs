using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MistakeCollectionSystem.Infrastructure.Data;
using MistakeCollectionSystem.Infrastructure.Services;
using MistakeCollectionSystem.Infrastructure.Services.AI;
using MistakeCollectionSystem.Shared;
using Serilog;
using System.Text;

namespace MistakeCollectionSystem.API;

/// <summary>
/// 应用程序入口类
/// 配置和启动 ASP.NET Core Web API 服务
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        // 配置 Serilog 日志
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/mistake-collection-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        try
        {
            Log.Information("启动错题集系统...");
            var builder = CreateHostBuilder(args);
            var app = builder.Build();

            // 自动迁移数据库（仅开发环境）
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "应用程序启动失败");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// 创建主机构建器
    /// </summary>
    public static WebApplicationBuilder CreateHostBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. 配置 Serilog
        builder.Host.UseSerilog();

        // 2. 配置服务
        ConfigureServices(builder);

        return builder;
    }

    /// <summary>
    /// 配置所有服务依赖注入
    /// </summary>
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // 添加控制器服务
        builder.Services.AddControllers();

        // 添加 API 端点探索器
        builder.Services.AddEndpointsApiExplorer();

        // 配置 Swagger/OpenAPI
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "错题集系统 API",
                Version = "v1",
                Description = "基于 AI 的错题识别与管理系统",
                Contact = new OpenApiContact
                {
                    Name = "开发团队",
                    Email = "dev@example.com"
                }
            });

            // 添加 JWT 认证配置
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        // 配置数据库连接
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

        // 配置 HTTP 客户端
        builder.Services.AddHttpClient<IAIParser, TongyiQianwenParser>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // 注册业务服务
        builder.Services.AddScoped<IMistakeService, MistakeService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        // 配置 CORS（跨域资源共享）
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(Constants.Api.CorsPolicyName, policy =>
            {
                policy.WithOrigins(builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:3000" })
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // 配置 JWT 认证
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key 未配置")))
                };
            });

        // 添加健康检查
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        // 添加内存缓存
        builder.Services.AddMemoryCache();

        // 添加响应压缩
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });
    }
}
