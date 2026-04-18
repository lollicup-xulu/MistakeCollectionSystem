using Microsoft.EntityFrameworkCore;
using MistakeCollectionSystem.Core.Entities;

namespace MistakeCollectionSystem.Infrastructure.Data
{
    /// <summary>
    /// 应用程序数据库上下文
    /// 负责与数据库的交互，管理所有实体的映射
    /// </summary>
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {

        /// <summary>
        /// 用户表
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// 错题主表
        /// </summary>
        public DbSet<MistakeQuestion> MistakeQuestions { get; set; }

        /// <summary>
        /// 原始记录表
        /// </summary>
        public DbSet<MistakeRawRecord> MistakeRawRecords { get; set; }

        /// <summary>
        /// 练习记录表
        /// </summary>
        public DbSet<PracticeRecord> PracticeRecords { get; set; }

        /// <summary>
        /// 错题集表
        /// </summary>
        public DbSet<MistakeCollection> MistakeCollections { get; set; }

        /// <summary>
        /// 配置实体关系和约束
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置 User 实体
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();
            });

            // 配置 MistakeQuestion 实体
            modelBuilder.Entity<MistakeQuestion>(entity =>
            {
                entity.HasIndex(q => new { q.UserId, q.Subject });
                entity.HasIndex(q => q.KnowledgePoints);
                entity.HasIndex(q => q.IsActive);

                entity.Property(q => q.MistakeCount)
                    .HasDefaultValue(1);

                entity.Property(q => q.DifficultyLevel)
                    .HasDefaultValue(3);

                entity.Property(q => q.ImportanceLevel)
                    .HasDefaultValue(3);

                // 配置关系
                entity.HasOne(q => q.User)
                    .WithMany(u => u.MistakeQuestions)
                    .HasForeignKey(q => q.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(q => q.RawRecord)
                    .WithMany(r => r.MistakeQuestions)
                    .HasForeignKey(q => q.RawRecordId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // 配置 PracticeRecord 实体
            modelBuilder.Entity<PracticeRecord>(entity =>
            {
                entity.HasIndex(p => p.PracticeDate);

                entity.HasOne(p => p.Question)
                    .WithMany(q => q.PracticeRecords)
                    .HasForeignKey(p => p.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置 MistakeCollection 实体
            modelBuilder.Entity<MistakeCollection>(entity =>
            {
                entity.Property(c => c.QuestionIds)
                    .HasColumnType("nvarchar(max)");

                entity.HasIndex(c => c.GenerateToken).IsUnique();
            });

            // 配置全局查询过滤器（软删除）
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<MistakeQuestion>().HasQueryFilter(q => !q.IsDeleted);
            modelBuilder.Entity<MistakeRawRecord>().HasQueryFilter(r => !r.IsDeleted);
        }

        /// <summary>
        /// 保存更改时自动更新时间戳
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdateTimestamp();
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
