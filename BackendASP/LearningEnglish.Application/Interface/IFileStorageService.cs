namespace LearningEnglish.Application.Interface
{
    public interface IFileStorageService
    {
        Task<string TempKey , > UploadFileTemplateAsync(IFormFile file);
    }
}