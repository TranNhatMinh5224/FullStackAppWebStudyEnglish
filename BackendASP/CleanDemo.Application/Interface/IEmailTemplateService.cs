namespace CleanDemo.Application.Interface
{
    public interface IEmailTemplateService
    {
        string GenerateOTPEmailTemplate(string otpCode, string userName);
        string GenerateWelcomeEmailTemplate(string userName);
        string GeneratePasswordChangedEmailTemplate(string userName);
        string GenerateNotifyJoinCourseTemplate(string courseName, string userName);
        string GenerateTeacherPackagePurchaseTemplate(string packageName, string userName, decimal price, DateTime validUntil);
    }
}