namespace MistakeCollectionSystem.Infrastructure.Services;

/// <summary>
/// 文件存储服务接口
/// 处理图片上传、存储和缩略图生成
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// 保存图片
    /// </summary>
    /// <param name="imageStream">图片流</param>
    /// <param name="fileName">原始文件名</param>
    /// <param name="userId">用户ID</param>
    /// <returns>保存的图片URL</returns>
    Task<string> SaveImageAsync(Stream imageStream, string fileName, int userId);

    /// <summary>
    /// 创建缩略图
    /// </summary>
    /// <param name="imageStream">图片流</param>
    /// <param name="fileName">原始文件名</param>
    /// <param name="userId">用户ID</param>
    /// <returns>缩略图URL</returns>
    Task<string> CreateThumbnailAsync(Stream imageStream, string fileName, int userId);

    /// <summary>
    /// 删除图片
    /// </summary>
    /// <param name="imageUrl">图片URL</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteImageAsync(string imageUrl);

    /// <summary>
    /// 获取图片的物理路径
    /// </summary>
    /// <param name="imageUrl">图片URL</param>
    /// <returns>物理路径</returns>
    string GetPhysicalPath(string imageUrl);
}