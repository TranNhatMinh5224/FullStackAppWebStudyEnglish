namespace CleanDemo.Application.Interface
{
    public interface ITemplatePathResolver
    {
        string GetTemplatePath(string templateName);
        bool TemplateExists(string templateName);
    }
}
