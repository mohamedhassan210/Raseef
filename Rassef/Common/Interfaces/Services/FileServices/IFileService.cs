public interface IFileService
{
    Task<string> UploadImageAsync(IFormFile file);

    void DeleteImage(string? imagePath);
}