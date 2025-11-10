using LearningEnglish.Application.Interface;

namespace LearningEnglish.Infrastructure.Services
{
    public class TemplatePathResolver : ITemplatePathResolver
    {
        private readonly string _basePath;

        public TemplatePathResolver()
        {
            // Look for templates in the Infrastructure project's Templates folder
            _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "CleanDemo.Infrastructure", "Templates");

            // Fallback to current directory if development path doesn't exist
            if (!Directory.Exists(_basePath))
            {
                _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
            }
        }

        public string GetTemplatePath(string templateName)
        {
            return Path.Combine(_basePath, templateName);
        }

        public bool TemplateExists(string templateName)
        {
            return File.Exists(GetTemplatePath(templateName));
        }
    }
}
