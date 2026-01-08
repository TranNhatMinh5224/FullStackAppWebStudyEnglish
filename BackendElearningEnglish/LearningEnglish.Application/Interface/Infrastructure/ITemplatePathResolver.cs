namespace LearningEnglish.Application.Interface
{
    public interface ITemplatePathResolver
    {
        // Lấy đường dẫn template
        string GetTemplatePath(string templateName);
        
        // Kiểm tra template tồn tại
        bool TemplateExists(string templateName);
    }
}
