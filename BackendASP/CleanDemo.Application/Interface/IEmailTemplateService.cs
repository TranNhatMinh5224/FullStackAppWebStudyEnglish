namespace CleanDemo.Application.Interface
{
    public interface IEmailTemplateService
    {
        string GenerateOTPEmailTemplate(string otpCode, string userName);
        string GenerateWelcomeEmailTemplate(string userName);
        string GeneratePasswordChangedEmailTemplate(string userName);
    }
}