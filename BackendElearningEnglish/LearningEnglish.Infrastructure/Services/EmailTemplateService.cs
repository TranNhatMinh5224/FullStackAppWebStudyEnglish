using LearningEnglish.Application.Interface;

namespace LearningEnglish.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly ITemplatePathResolver _pathResolver;

        public EmailTemplateService(ITemplatePathResolver pathResolver)
        {
            _pathResolver = pathResolver;
        }

        public string GenerateOTPEmailTemplate(string otpCode, string userName)
        {
            var templatePath = _pathResolver.GetTemplatePath("OTPEmail.html");

            if (!_pathResolver.TemplateExists("OTPEmail.html"))
            {
                throw new FileNotFoundException($"Email template not found: {templatePath}");
            }

            var template = File.ReadAllText(templatePath);

            // Replace placeholders
            return template
                .Replace("{{OTPCode}}", otpCode)
                .Replace("{{UserName}}", userName);
        }

        public string GenerateWelcomeEmailTemplate(string userName)
        {
            // Future template for welcome emails
            return $"<h1>Welcome {userName}!</h1><p>Thank you for joining English Learning App!</p>";
        }

        public string GeneratePasswordChangedEmailTemplate(string userName)
        {
            // Future template for password change notifications
            return $"<h1>Password Changed</h1><p>Hello {userName}, your password has been successfully changed.</p>";
        }
        public string GenerateNotifyJoinCourseTemplate(string courseName, string userName)
        {
            var templatePath = _pathResolver.GetTemplatePath("CoursePurchaseConfirmation.html");
            var htmlTemplate = File.ReadAllText(templatePath);

            return htmlTemplate
                .Replace("{{USER_NAME}}", userName)
                .Replace("{{COURSE_NAME}}", courseName)
                .Replace("{{PURCHASE_DATE}}", DateTime.UtcNow.ToString("dd/MM/yyyy"))
                .Replace("{{COURSE_URL}}", $"https://catalunya-english.com/courses/{courseName.Replace(" ", "-").ToLower()}")
                .Replace("{{CURRENT_YEAR}}", DateTime.UtcNow.Year.ToString());
        }

        public string GenerateTeacherPackagePurchaseTemplate(string packageName, string userName, decimal price, DateTime validUntil)
        {
            var templatePath = _pathResolver.GetTemplatePath("TeacherPackagePurchase.html");
            var htmlTemplate = File.ReadAllText(templatePath);

            return htmlTemplate
                .Replace("{{USER_NAME}}", userName)
                .Replace("{{PACKAGE_NAME}}", packageName)
                .Replace("{{PRICE}}", price.ToString("F2"))
                .Replace("{{PURCHASE_DATE}}", DateTime.UtcNow.ToString("dd/MM/yyyy"))
                .Replace("{{VALID_UNTIL}}", validUntil.ToString("dd/MM/yyyy"))
                .Replace("{{TEACHER_DASHBOARD_URL}}", "https://catalunya-english.com/teacher/dashboard")
                .Replace("{{CURRENT_YEAR}}", DateTime.UtcNow.Year.ToString());
        }

        public string GenerateVocabularyReminderTemplate(string studentName, int dueCount)
        {
            var templatePath = _pathResolver.GetTemplatePath("VocabularyReminder.html");

            if (!_pathResolver.TemplateExists("VocabularyReminder.html"))
            {
                throw new FileNotFoundException($"Email template not found: {templatePath}");
            }

            var htmlTemplate = File.ReadAllText(templatePath);

            // Tạo nội dung động dựa vào số lượng từ vựng
            var content = dueCount switch
            {
                1 => "You have 1 vocabulary word to review today. Spaced Repetition helps you remember longer!",
                <= 5 => $"Today you have {dueCount} vocabulary words to review. This is the best time to consolidate knowledge scientifically!",
                <= 10 => $"You have {dueCount} vocabulary words to review. The Spaced Repetition System has calculated the optimal time for memorization!",
                _ => $"Today you have {dueCount} vocabulary words to review. Don't miss this opportunity to improve your English!"
            };

            return htmlTemplate
                .Replace("{{StudentName}}", studentName)
                .Replace("{{DueCount}}", dueCount.ToString())
                .Replace("{{Content}}", content)
                .Replace("{{ReviewUrl}}", "https://catalunya-english.com/flashcards/review");
        }

        public string GenerateStreakReminderTemplate(string userName, int currentStreak, int longestStreak)
        {
            var isNewRecord = currentStreak >= longestStreak;
            
            var motivationMessage = currentStreak switch
            {
                >= 30 => $"🏆 Bạn đã giữ streak {currentStreak} ngày! Đây là một thành tích tuyệt vời. Đừng để nỗ lực này mất phí!",
                >= 14 => $"🔥 Streak {currentStreak} ngày của bạn đang rất ấn tượng! Chỉ cần vài phút học hôm nay để tiếp tục!",
                >= 7 => $"⭐ {currentStreak} ngày liên tiếp! Bạn đang xây dựng thói quen học tập tuyệt vời. Hãy tiếp tục!",
                _ => $"💪 Streak {currentStreak} ngày của bạn đang trong nguy hiểm! Hãy dành ít phút học hôm nay."
            };

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #FF6B6B 0%, #FFE66D 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .streak-box {{ background: white; padding: 20px; margin: 20px 0; border-radius: 10px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }}
        .button {{ display: inline-block; padding: 15px 30px; background: #FF6B6B; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .stats {{ display: flex; justify-content: space-around; margin: 20px 0; }}
        .stat {{ text-align: center; }}
        .stat-number {{ font-size: 32px; font-weight: bold; color: #FF6B6B; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔥 Streak Reminder!</h1>
            <p>Đừng để streak của bạn đứt!</p>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            
            <div class='streak-box'>
                <h2 style='color: #FF6B6B; text-align: center;'>⚠️ Cảnh báo Streak!</h2>
                <p style='text-align: center; font-size: 18px;'>{motivationMessage}</p>
                
                <div class='stats'>
                    <div class='stat'>
                        <div class='stat-number'>🔥 {currentStreak}</div>
                        <div>Current Streak</div>
                    </div>
                    <div class='stat'>
                        <div class='stat-number'>🏆 {longestStreak}</div>
                        <div>Longest Streak</div>
                    </div>
                </div>
            </div>

            <p><strong>Tại sao streak quan trọng?</strong></p>
            <ul>
                <li>✅ Học đều đặn giúp bạn nhớ lâu hơn</li>
                <li>✅ Xây dựng thói quen học tập bền vững</li>
                <li>✅ Cảm giác thành tựu khi duy trì streak</li>
                <li>✅ Động lực tiếp tục phát triển mỗi ngày</li>
            </ul>

            <p style='text-align: center;'>
                <a href='https://catalunya-english.com/learn' class='button'>
                    Học ngay để giữ streak 🚀
                </a>
            </p>

            <p style='color: #666; font-size: 14px;'>
                💡 <em>Chỉ cần 5-10 phút học hôm nay là bạn đã giữ được streak rồi!</em>
            </p>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Catalunya English Learning Platform</p>
            <p>You received this email because you have an active learning streak.</p>
        </div>
    </div>
</body>
</html>";

            return html;
        }
    }
}
