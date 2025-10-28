namespace CleanDemo.Application.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendOTPEmailAsync(string toEmail, string otpCode, string userName);
    }
}