using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MistakeCollectionSystem.Shared;
using System.Drawing;
using System.Drawing.Imaging;


namespace MistakeCollectionSystem.Infrastructure.Services
{


    /// <summary>
    /// 文件存储服务实现
    /// 使用本地文件系统存储图片
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _basePath;
        private readonly string _imagePath;
        private readonly string _thumbnailPath;

        public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _basePath = _configuration["FileStorage:BasePath"] ?? "uploads";
            _imagePath = _configuration["FileStorage:ImagePath"] ?? "images";
            _thumbnailPath = _configuration["FileStorage:ThumbnailPath"] ?? "thumbnails";

            // 确保目录存在
            EnsureDirectoriesExist();
        }

        /// <summary>
        /// 确保存储目录存在
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            var baseDir = Path.Combine(Directory.GetCurrentDirectory(), _basePath);
            var imageDir = Path.Combine(baseDir, _imagePath);
            var thumbnailDir = Path.Combine(baseDir, _thumbnailPath);

            Directory.CreateDirectory(imageDir);
            Directory.CreateDirectory(thumbnailDir);
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        public async Task<string> SaveImageAsync(Stream imageStream, string fileName, int userId)
        {
            try
            {
                // 生成唯一文件名
                var extension = Path.GetExtension(fileName);
                var uniqueFileName = $"{userId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";

                // 按日期分目录存储
                var dateFolder = DateTime.Now.ToString("yyyyMMdd");
                var userFolder = userId.ToString();

                var relativePath = Path.Combine(_imagePath, dateFolder, userFolder);
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), _basePath, relativePath);

                Directory.CreateDirectory(fullPath);

                var filePath = Path.Combine(fullPath, uniqueFileName);

                // 重置流位置
                if (imageStream.CanSeek)
                {
                    imageStream.Position = 0;
                }

                // 压缩并保存图片
                using var image = Image.FromStream(imageStream);
                var encoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, Constants.FileStorage.ImageQuality);

                // 转换为JPEG格式保存
                var jpegPath = Path.ChangeExtension(filePath, ".jpg");
                image.Save(jpegPath, encoder, encoderParams);

                // 返回相对URL
                var url = $"/{_basePath}/{relativePath}/{Path.GetFileName(jpegPath)}".Replace("\\", "/");

                _logger.LogInformation("图片已保存: {Url}", url);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存图片失败");
                throw;
            }
        }

        /// <summary>
        /// 创建缩略图
        /// </summary>
        public async Task<string> CreateThumbnailAsync(Stream imageStream, string fileName, int userId)
        {
            try
            {
                // 重置流位置
                if (imageStream.CanSeek)
                {
                    imageStream.Position = 0;
                }

                using var originalImage = Image.FromStream(imageStream);

                // 计算缩略图尺寸（保持宽高比）
                var ratio = Math.Min(
                    Constants.FileStorage.ThumbnailMaxWidth / (float)originalImage.Width,
                    Constants.FileStorage.ThumbnailMaxHeight / (float)originalImage.Height);

                var thumbWidth = (int)(originalImage.Width * ratio);
                var thumbHeight = (int)(originalImage.Height * ratio);

                // 创建缩略图
                using var thumbnail = new Bitmap(thumbWidth, thumbHeight);
                using var graphics = Graphics.FromImage(thumbnail);
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(originalImage, 0, 0, thumbWidth, thumbHeight);

                // 生成文件名
                var extension = Path.GetExtension(fileName);
                var uniqueFileName = $"{userId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}_thumb.jpg";

                var dateFolder = DateTime.Now.ToString("yyyyMMdd");
                var userFolder = userId.ToString();

                var relativePath = Path.Combine(_thumbnailPath, dateFolder, userFolder);
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), _basePath, relativePath);

                Directory.CreateDirectory(fullPath);

                var filePath = Path.Combine(fullPath, uniqueFileName);

                // 保存缩略图
                var encoder = GetEncoder(ImageFormat.Jpeg);

                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 75L);
                thumbnail.Save(filePath, encoder, encoderParams);


                // 返回相对URL
                var url = $"/{_basePath}/{relativePath}/{uniqueFileName}".Replace("\\", "/");

                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建缩略图失败");
                return string.Empty; // 缩略图创建失败不影响主流程
            }
        }

        /// <summary>
        /// 删除图片
        /// </summary>
        public Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                var relativePath = imageUrl.TrimStart('/');
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("图片已删除: {Path}", fullPath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除图片失败: {Url}", imageUrl);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// 获取图片的物理路径
        /// </summary>
        public string GetPhysicalPath(string imageUrl)
        {
            var relativePath = imageUrl.TrimStart('/');
            return Path.Combine(Directory.GetCurrentDirectory(), relativePath);
        }

        /// <summary>
        /// 获取图片编码器
        /// </summary>
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid)
                ?? throw new InvalidOperationException($"未找到 {format} 编码器");
        }
    }
}
