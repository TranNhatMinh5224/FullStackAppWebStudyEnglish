namespace LearningEnglish.Application.Interface
{
    public interface IEmailSender
    {
        // Gửi email
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
