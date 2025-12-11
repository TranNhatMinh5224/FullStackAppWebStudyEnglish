using LearningEnglish.Application.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Application.Service;

/// <summary>
/// Service chuyên biệt CHỈ NHẮC HỌC LẠI TỪ VỰNG qua App + Email
/// Mục đích duy nhất: Nhắc user ôn tập từ vựng đã học theo lịch trình SRS
/// </summary>
public class VocabularyReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VocabularyReminderService> _logger;
    private readonly IConfiguration _configuration;

    public VocabularyReminderService(
        IServiceProvider serviceProvider,
        ILogger<VocabularyReminderService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 VocabularyReminderService khởi động - CHỈ NHẮC HỌC TỪ VỰNG");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var targetTime = new TimeSpan(12, 0, 0); // 12:00 UTC = 19:00 VN (giờ vàng)
                
                if (ShouldSendReminder(now, targetTime))
                {
                    await SendVocabularyReminders();
                    
                    // Chờ 24h cho lần tiếp theo
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                else
                {
                    // Kiểm tra lại sau 1 giờ
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Task bị cancel khi service shutdown - đây là hành vi bình thường
                _logger.LogInformation("⏹️ VocabularyReminderService đang shutdown...");
                break;
            }
            catch (OperationCanceledException)
            {
                // Task bị cancel khi service shutdown - đây là hành vi bình thường
                _logger.LogInformation("⏹️ VocabularyReminderService đang shutdown...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi trong VocabularyReminderService");
                
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("⏹️ VocabularyReminderService đang shutdown sau lỗi...");
                    break;
                }
            }
        }

        _logger.LogInformation("✅ VocabularyReminderService đã dừng");
    }

    /// <summary>
    /// GỬI NHẮC NHỞ HỌC TỪ VỰNG qua App + Email
    /// </summary>
    private async Task SendVocabularyReminders()
    {
        _logger.LogInformation("📚 Bắt đầu gửi nhắc nhở học từ vựng...");

        using var scope = _serviceProvider.CreateScope();
        var reviewRepository = scope.ServiceProvider.GetRequiredService<IFlashCardReviewRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<SimpleNotificationService>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        try
        {
            var currentDate = DateTime.UtcNow.Date;
            
            // Lấy tất cả users và filter students
            var allUsers = await userRepository.GetAllUsersAsync();
            var students = allUsers.Where(u => u.Roles.Any(r => r.Name == "Student")).ToList();
            
            int sentAppNotifications = 0;
            int sentEmails = 0;

            foreach (var student in students)
            {
                // Đếm từ vựng cần ôn hôm nay
                var dueCount = await reviewRepository.GetDueCountAsync(student.UserId, currentDate);
                
                if (dueCount > 0)
                {
                    var reminderData = CreateReminderContent(dueCount, student.FullName ?? "bạn");

                    // 1. GỬI THÔNG BÁO TRONG APP
                    await notificationService.CreateNotificationAsync(
                        userId: student.UserId,
                        title: reminderData.AppTitle,
                        message: reminderData.AppContent
                    );
                    sentAppNotifications++;

                    // 2. GỬI EMAIL (nếu có email)
                    if (!string.IsNullOrEmpty(student.Email))
                    {
                        var emailSent = await SendReminderEmail(
                            studentEmail: student.Email,
                            studentName: student.FullName ?? "Học viên",
                            dueCount: dueCount,
                            emailContent: reminderData.EmailContent
                        );

                        if (emailSent) sentEmails++;
                    }

                    _logger.LogDebug("📤 Gửi nhắc nhở cho {Email}: {Count} từ vựng", 
                        student.Email, dueCount);
                }
            }

            _logger.LogInformation("✅ Đã gửi {AppCount} thông báo app và {EmailCount} email nhắc học từ vựng", 
                sentAppNotifications, sentEmails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Lỗi khi gửi nhắc nhở học từ vựng");
        }
    }

    /// <summary>
    /// GỬI EMAIL NHẮC NHỞ HỌC TỪ VỰNG
    /// </summary>
    private async Task<bool> SendReminderEmail(string studentEmail, string studentName, int dueCount, string emailContent)
    {
        try
        {
            var smtpHost = _configuration["SmtpOptions:Host"];
            var smtpPort = int.Parse(_configuration["SmtpOptions:Port"] ?? "587");
            var smtpEmail = _configuration["SmtpOptions:Email"];
            var smtpPassword = _configuration["SmtpOptions:Password"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpEmail))
            {
                _logger.LogWarning("⚠️ SMTP không được cấu hình, bỏ qua gửi email");
                return false;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpEmail, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpEmail, "English Learning App"),
                Subject = $"📚 {dueCount} từ vựng cần ôn tập hôm nay!",
                Body = CreateEmailBody(studentName, dueCount, emailContent),
                IsBodyHtml = true
            };

            mailMessage.To.Add(studentEmail);

            await client.SendMailAsync(mailMessage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Lỗi gửi email cho {Email}", studentEmail);
            return false;
        }
    }

    #region Private Helper Methods

    private bool ShouldSendReminder(DateTime now, TimeSpan targetTime)
    {
        return now.TimeOfDay >= targetTime && 
               now.TimeOfDay < targetTime.Add(TimeSpan.FromMinutes(30));
    }

    private (string AppTitle, string AppContent, string EmailContent) CreateReminderContent(int dueCount, string studentName)
    {
        var appTitle = dueCount switch
        {
            1 => "📚 1 từ vựng cần ôn!",
            <= 5 => $"📚 {dueCount} từ vựng cần ôn!",
            <= 10 => $"📚 {dueCount} từ vựng đang chờ bạn!",
            <= 20 => $"📚 Wow! {dueCount} từ vựng cần ôn tập!",
            _ => $"📚 {dueCount} từ vựng - Thời gian ôn tập đây!"
        };

        var appContent = dueCount switch
        {
            1 => "Bạn có 1 từ vựng cần ôn tập hôm nay. Chỉ mất vài giây thôi! 🚀",
            <= 5 => $"Bạn có {dueCount} từ vựng cần ôn tập hôm nay. Hãy dành 5 phút để ghi nhớ tốt hơn nhé! 🧠✨",
            <= 10 => $"Bạn có {dueCount} từ vựng cần ôn tập hôm nay. Dành 10-15 phút để ôn sẽ giúp bạn nhớ lâu hơn! 📚",
            <= 20 => $"Bạn có {dueCount} từ vựng cần ôn tập hôm nay. Đây là cơ hội tuyệt vời để củng cố kiến thức! 🎯",
            _ => $"Bạn có {dueCount} từ vựng cần ôn tập. Hãy chia nhỏ thành nhiều lần trong ngày để hiệu quả hơn! 💪"
        };

        var emailContent = dueCount switch
        {
            1 => "Bạn có 1 từ vựng cần ôn tập hôm nay. Spaced Repetition giúp bạn nhớ lâu hơn!",
            <= 5 => $"Hôm nay bạn có {dueCount} từ vựng cần ôn tập. Đây là thời điểm tốt nhất để củng cố kiến thức theo khoa học!",
            <= 10 => $"Bạn có {dueCount} từ vựng cần ôn tập. Hệ thống Spaced Repetition đã tính toán thời gian tối ưu cho việc ghi nhớ!",
            _ => $"Hôm nay bạn có {dueCount} từ vựng cần ôn tập. Đừng bỏ lỡ cơ hội này để nâng cao khả năng tiếng Anh!"
        };

        return (appTitle, appContent, emailContent);
    }

    private string CreateEmailBody(string studentName, int dueCount, string content)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; }}
        .highlight {{ background: #e3f2fd; padding: 15px; border-left: 4px solid #2196f3; margin: 20px 0; }}
        .cta {{ text-align: center; margin: 30px 0; }}
        .btn {{ background: #4CAF50; color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; }}
        .footer {{ text-align: center; color: #666; font-size: 14px; padding: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📚 Nhắc nhở học từ vựng</h1>
            <h2>Chào {studentName}!</h2>
        </div>
        
        <div class='content'>
            <div class='highlight'>
                <h3>🎯 Bạn có <strong>{dueCount} từ vựng</strong> cần ôn tập hôm nay!</h3>
                <p>{content}</p>
            </div>
            
            <p>📈 <strong>Spaced Repetition System</strong> đã tính toán thời gian tối ưu để bạn ghi nhớ những từ vựng này.</p>
            
            <p>💡 <strong>Tại sao nên ôn tập ngay hôm nay?</strong></p>
            <ul>
                <li>🧠 Tăng cường trí nhớ dài hạn</li>
                <li>⚡ Chỉ mất 5-15 phút</li>
                <li>📊 Nâng cao hiệu quả học tập</li>
                <li>🎯 Đạt được mục tiêu học tiếng Anh</li>
            </ul>
            
            <div class='cta'>
                <a href='#' class='btn'>🚀 Bắt đầu ôn tập ngay!</a>
            </div>
        </div>
        
        <div class='footer'>
            <p>📱 English Learning App | Học tiếng Anh thông minh với khoa học</p>
            <p><small>Email này được gửi tự động. Bạn nhận được vì có từ vựng cần ôn tập hôm nay.</small></p>
        </div>
    </div>
</body>
</html>";
    }

    #endregion
}
