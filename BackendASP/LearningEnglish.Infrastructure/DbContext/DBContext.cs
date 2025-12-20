using Microsoft.EntityFrameworkCore;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();

        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<Lecture> Lectures => Set<Lecture>();
        public DbSet<FlashCard> FlashCards => Set<FlashCard>();
        public DbSet<Assessment> Assessments => Set<Assessment>();
        public DbSet<Quiz> Quizzes => Set<Quiz>();
        public DbSet<QuizSection> QuizSections => Set<QuizSection>();
        public DbSet<QuizGroup> QuizGroups => Set<QuizGroup>();
        public DbSet<Essay> Essays => Set<Essay>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
        public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<ModuleCompletion> ModuleCompletions => Set<ModuleCompletion>();
        public DbSet<LessonCompletion> LessonCompletions => Set<LessonCompletion>();
        public DbSet<CourseProgress> CourseProgresses => Set<CourseProgress>();
        public DbSet<FlashCardReview> FlashCardReviews => Set<FlashCardReview>();
        public DbSet<PronunciationProgress> PronunciationProgresses => Set<PronunciationProgress>();
        public DbSet<Streak> Streaks => Set<Streak>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
        public DbSet<EssaySubmission> EssaySubmissions => Set<EssaySubmission>();
        public DbSet<UserCourse> UserCourses => Set<UserCourse>();
        public DbSet<TeacherPackage> TeacherPackages => Set<TeacherPackage>();
        public DbSet<TeacherSubscription> TeacherSubscriptions => Set<TeacherSubscription>();
        public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();



        // ============================================================================
        // ROW-LEVEL SECURITY (RLS) - BẢO MẬT CẤP HÀNG (ROW) TRONG DATABASE
        // ============================================================================
        
        // Method này thiết lập context cho RLS trong PostgreSQL
        // Được gọi TỰ ĐỘNG bởi RlsMiddleware ở đầu mỗi HTTP request
        //
        // CÁCH HOẠT ĐỘNG:
        // 1. RlsMiddleware extract userId + role từ JWT token
        // 2. Gọi method này để set 2 session variables trong PostgreSQL:
        //    - app.current_user_id: Lưu ID của user hiện tại (VD: 123)
        //    - app.current_user_role: Lưu role (Admin, Teacher, hoặc Student)
        // 3. RLS policies trong database sẽ dùng 2 variables này để filter data tự động
        //
        // VÍ DỤ FLOW:
        // Request 1 (Teacher userId=5):
        //   → BEGIN TRANSACTION
        //   → SET LOCAL app.current_user_id = '5'
        //   → SET LOCAL app.current_user_role = 'Teacher'
        //   → Query: SELECT * FROM "Courses"
        //   → RLS tự động filter: WHERE "TeacherId" = 5
        //   → COMMIT → Variables bị xóa tự động ✓
        //
        // Request 2 (Student userId=10) - reuse cùng connection:
        //   → BEGIN NEW TRANSACTION
        //   → SET LOCAL app.current_user_id = '10'
        //   → SET LOCAL app.current_user_role = 'Student'
        //   → Query: SELECT * FROM "Courses"
        //   → RLS tự động filter: WHERE EXISTS (SELECT 1 FROM "UserCourses"...)
        //   → COMMIT → Variables bị xóa tự động ✓
        //
        // TẠI SAO AN TOÀN VỚI CONNECTION POOLING:
        // - Parameter thứ 3 (true) trong set_config() = LOCAL scope
        // - Variables CHỈ tồn tại trong transaction hiện tại
        // - Tự động cleared sau COMMIT/ROLLBACK
        // - Connection có thể reuse an toàn cho user khác
        public async Task SetUserContextAsync(int userId, string role)
        {
            // ExecuteSqlRawAsync: Thực thi raw SQL command trên database
            //
            // set_config(name, value, is_local):
            //   - name: Tên variable cần set
            //   - value: Giá trị
            //   - is_local: true = LOCAL (chỉ trong transaction), false = SESSION (toàn bộ session)
            //
            // ⚠️ QUAN TRỌNG: PHẢI dùng is_local=true để an toàn với connection pooling!
            // Nếu dùng false, variable sẽ tồn tại suốt session → nguy hiểm khi reuse connection
            await Database.ExecuteSqlRawAsync(
                "SELECT set_config('app.current_user_id', {0}, true), set_config('app.current_user_role', {1}, true)",
                userId.ToString(),  // {0} - Convert userId sang string
                role                // {1} - Role name (Admin, Teacher, Student)
            );
            
            // Sau khi execute xong:
            // PostgreSQL session hiện tại có 2 variables:
            //   - current_setting('app.current_user_id', true) → trả về "123"
            //   - current_setting('app.current_user_role', true) → trả về "Teacher"
            //
            // RLS policies sẽ dùng current_setting() để lấy giá trị và filter data
            // VD: WHERE "TeacherId" = current_setting('app.current_user_id', true)::integer
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== User =====
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");

                // Column constraints (sync with Entity & Validators)
                e.Property(u => u.FirstName)
                 .IsRequired()
                 .HasMaxLength(100);

                e.Property(u => u.LastName)
                 .IsRequired()
                 .HasMaxLength(100);

                e.Property(u => u.Email)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(u => u.NormalizedEmail)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(u => u.PasswordHash)
                 .HasMaxLength(255);

                e.Property(u => u.PhoneNumber)
                 .IsRequired()
                 .HasMaxLength(20);

                e.Property(u => u.AvatarKey)
                 .HasMaxLength(500);

                e.Property(u => u.DateOfBirth)
                 .IsRequired(false);

                e.Property(u => u.IsMale)
                 .IsRequired()
                 .HasDefaultValue(true);

                e.Property(u => u.Status)
                 .IsRequired();

                e.Property(u => u.EmailVerified)
                 .IsRequired()
                 .HasDefaultValue(false);

                e.Property(u => u.CreatedAt)
                 .IsRequired();

                e.Property(u => u.UpdatedAt)
                 .IsRequired();

                // Unique indexes
                e.HasIndex(u => u.Email).IsUnique();
                e.HasIndex(u => u.NormalizedEmail).IsUnique();

                // Query optimization indexes
                e.HasIndex(u => u.Status);
                e.HasIndex(u => u.EmailVerified);
                e.HasIndex(u => u.CreatedAt);

                // Composite indexes for complex queries
                e.HasIndex(u => new { u.Status, u.CreatedAt })
                 .HasDatabaseName("IX_User_Status_CreatedAt");

                e.HasIndex(u => new { u.EmailVerified, u.CreatedAt })
                 .HasDatabaseName("IX_User_EmailVerified_CreatedAt");

                e.HasOne(u => u.CurrentTeacherSubscription)
                 .WithMany()
                 .HasForeignKey(u => u.CurrentTeacherSubscriptionId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== Role =====
            modelBuilder.Entity<Role>(e =>
            {
                e.ToTable("Roles");

                e.Property(r => r.Name)
                 .IsRequired()
                 .HasMaxLength(50);

                e.HasIndex(r => r.Name).IsUnique();
            });

            // ===== User-Role Many-to-Many =====
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity(
                    "UserRoles",
                    l => l.HasOne(typeof(Role)).WithMany().HasForeignKey("RoleId"),
                    r => r.HasOne(typeof(User)).WithMany().HasForeignKey("UserId"),
                    j => j.HasKey("UserId", "RoleId"));

            // Course
            modelBuilder.Entity<Course>(e =>
            {
                e.ToTable("Courses");

                // Column constraints
                e.Property(c => c.Title)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(c => c.DescriptionMarkdown)
                 .IsRequired()
                 .HasMaxLength(200000);

                e.Property(c => c.ImageKey)
                 .HasMaxLength(500);

                e.Property(c => c.ImageType)
                 .HasMaxLength(50);

                e.Property(c => c.ClassCode)
                 .HasMaxLength(20);

                e.Property(c => c.Type)
                 .IsRequired();

                e.Property(c => c.Status)
                 .IsRequired();

                e.Property(c => c.Price)
                 .HasPrecision(18, 2);

                e.Property(c => c.CreatedAt)
                 .IsRequired();

                // Indexes - Only essential ones
                e.HasIndex(c => c.Status);
                e.HasIndex(c => c.Type);

                e.HasIndex(e => e.Title)
                 .HasMethod("GIN")  //  sử dụng gin index 
                .HasOperators("gin_trgm_ops") // sử dụng gin_trgm_ops operator 
                .HasFilter("\"Type\" = 1")
                .HasDatabaseName("IX_Course_Title_SystemType"); // đặt tên index trong db 


                e.HasIndex(c => c.ClassCode);


                // Relationships
                e.HasOne(c => c.Teacher)
                 .WithMany(u => u.CreatedCourses)
                 .HasForeignKey(c => c.TeacherId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // Lesson
            modelBuilder.Entity<Lesson>(e =>
            {
                e.ToTable("Lessons");

                // Column constraints
                e.Property(l => l.Title)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(l => l.Description)
                 .HasMaxLength(1000);

                e.Property(l => l.ImageKey)
                 .HasMaxLength(500);

                e.Property(l => l.ImageType)
                 .HasMaxLength(50);

                e.Property(l => l.CreatedAt)
                 .IsRequired();

                e.Property(l => l.UpdatedAt)
                 .IsRequired();

                // Indexes
                e.HasIndex(l => new { l.CourseId, l.OrderIndex });

                // Relationships
                e.HasOne(l => l.Course)
                 .WithMany(c => c.Lessons)
                 .HasForeignKey(l => l.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Module
            modelBuilder.Entity<Module>(e =>
            {
                e.ToTable("Modules");

                // Column constraints
                e.Property(m => m.Name)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(m => m.Description)
                 .HasMaxLength(1000);

                e.Property(m => m.ImageKey)
                 .HasMaxLength(500);

                e.Property(m => m.ImageType)
                 .HasMaxLength(50);

                e.Property(m => m.ContentType)
                 .IsRequired();

                e.Property(m => m.CreatedAt)
                 .IsRequired();

                e.Property(m => m.UpdatedAt)
                 .IsRequired();

                // Indexes
                e.HasIndex(m => new { m.LessonId, m.OrderIndex });

                // Relationships
                e.HasOne(m => m.Lesson)
                 .WithMany(l => l.Modules)
                 .HasForeignKey(m => m.LessonId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Lecture
            modelBuilder.Entity<Lecture>(e =>
            {
                e.ToTable("Lectures");

                // Column constraints
                e.Property(l => l.Title)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(l => l.NumberingLabel)
                 .HasMaxLength(20);

                e.Property(l => l.Type)
                 .IsRequired();

                e.Property(l => l.MarkdownContent)
                 .HasMaxLength(50000);

                e.Property(l => l.RenderedHtml)
                 .IsRequired();

                e.Property(l => l.MediaKey)
                 .HasMaxLength(500);

                e.Property(l => l.MediaType)
                 .HasMaxLength(100);

                e.Property(l => l.CreatedAt)
                 .IsRequired();

                e.Property(l => l.UpdatedAt)
                 .IsRequired();

                // Indexes
                e.HasIndex(l => new { l.ModuleId, l.OrderIndex });

                // Relationships
                e.HasOne(l => l.Module)
                 .WithMany(m => m.Lectures)
                 .HasForeignKey(l => l.ModuleId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Self-referencing for hierarchical structure
                e.HasOne(l => l.Parent)
                 .WithMany(l => l.Children)
                 .HasForeignKey(l => l.ParentLectureId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // FlashCard
            modelBuilder.Entity<FlashCard>(e =>
            {
                e.ToTable("FlashCards");

                // Column constraints
                e.Property(fc => fc.Word)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(fc => fc.Meaning)
                 .IsRequired()
                 .HasMaxLength(500);

                e.Property(fc => fc.Pronunciation)
                 .HasMaxLength(100);

                e.Property(fc => fc.ImageKey)
                 .HasMaxLength(500);

                e.Property(fc => fc.AudioKey)
                 .HasMaxLength(500);

                e.Property(fc => fc.ImageType)
                 .HasMaxLength(50);

                e.Property(fc => fc.AudioType)
                 .HasMaxLength(50);

                e.Property(fc => fc.PartOfSpeech)
                 .HasMaxLength(50);

                e.Property(fc => fc.Example)
                 .HasMaxLength(1000);

                e.Property(fc => fc.ExampleTranslation)
                 .HasMaxLength(1000);

                e.Property(fc => fc.Synonyms)
                 .HasMaxLength(500);

                e.Property(fc => fc.Antonyms)
                 .HasMaxLength(500);

                e.Property(fc => fc.CreatedAt)
                 .IsRequired();

                e.Property(fc => fc.UpdatedAt)
                 .IsRequired();

                // Relationships
                e.HasOne(fc => fc.Module)
                 .WithMany(m => m.FlashCards)
                 .HasForeignKey(fc => fc.ModuleId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Assessment
            modelBuilder.Entity<Assessment>(e =>
            {
                e.ToTable("Assessments");

                // Column constraints
                e.Property(a => a.Title)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(a => a.Description)
                 .HasMaxLength(2000);

                e.Property(a => a.TotalPoints)
                 .HasPrecision(18, 2);

                e.HasOne(a => a.Module)
                 .WithMany(m => m.Assessments)
                 .HasForeignKey(a => a.ModuleId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Quiz
            modelBuilder.Entity<Quiz>(e =>
            {
                e.ToTable("Quizzes");

                // Column constraints
                e.Property(q => q.Title)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(q => q.Description)
                 .HasMaxLength(2000);

                e.Property(q => q.Instructions)
                 .HasMaxLength(2000);

                e.Property(q => q.TotalPossibleScore)
                 .HasPrecision(18, 2);

                e.HasOne(q => q.Assessment)
                 .WithMany(a => a.Quizzes)
                 .HasForeignKey(q => q.AssessmentId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // QuizSection
            modelBuilder.Entity<QuizSection>(e =>
            {
                e.ToTable("QuizSections");

                // Column constraints
                e.Property(qs => qs.Title)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(qs => qs.Description)
                 .HasMaxLength(1000);

                e.HasOne(qs => qs.Quiz)
                 .WithMany(q => q.QuizSections)
                 .HasForeignKey(qs => qs.QuizId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // QuizGroup
            modelBuilder.Entity<QuizGroup>(e =>
            {
                e.ToTable("QuizGroups");

                // Column constraints
                e.Property(qg => qg.Name)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(qg => qg.Description)
                 .HasMaxLength(1000);

                e.Property(qg => qg.Title)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(qg => qg.ImgKey)
                 .HasMaxLength(500);

                e.Property(qg => qg.VideoKey)
                 .HasMaxLength(500);

                e.Property(qg => qg.ImgType)
                 .HasMaxLength(50);

                e.Property(qg => qg.VideoType)
                 .HasMaxLength(50);

                e.HasOne(qg => qg.QuizSection)
                 .WithMany(qs => qs.QuizGroups)
                 .HasForeignKey(qg => qg.QuizSectionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Essay
            modelBuilder.Entity<Essay>(e =>
            {
                e.ToTable("Essays");

                // Column constraints
                e.Property(es => es.Title)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(es => es.Description)
                 .HasMaxLength(2000);

                e.Property(es => es.AudioKey)
                 .HasMaxLength(500);

                e.Property(es => es.AudioType)
                 .HasMaxLength(50);

                e.Property(es => es.ImageKey)
                 .HasMaxLength(500);

                e.Property(es => es.ImageType)
                 .HasMaxLength(50);

                e.HasOne(es => es.Assessment)
                 .WithMany(a => a.Essays)
                 .HasForeignKey(es => es.AssessmentId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Question
            modelBuilder.Entity<Question>(e =>
            {
                e.ToTable("Questions");

                // Column constraints
                e.Property(q => q.StemText)
                 .IsRequired()
                 .HasMaxLength(2000);

                e.Property(q => q.StemHtml)
                 .HasMaxLength(5000);

                e.Property(q => q.Points)
                 .HasPrecision(18, 2);

                e.Property(q => q.CorrectAnswersJson)
                 .HasMaxLength(2000);

                e.Property(q => q.Explanation)
                 .HasMaxLength(2000);

                e.Property(q => q.MediaKey)
                 .HasMaxLength(500);

                e.Property(q => q.MediaType)
                 .HasMaxLength(100);

                e.Property(q => q.MetadataJson)
                 .HasMaxLength(5000);

                e.HasOne(q => q.QuizSection)
                 .WithMany(qs => qs.Questions) // Giả sử QuizSection có List<Question>
                 .HasForeignKey(q => q.QuizSectionId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(q => q.QuizGroup)
                 .WithMany(qg => qg.Questions)
                 .HasForeignKey(q => q.QuizGroupId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // AnswerOption
            modelBuilder.Entity<AnswerOption>(e =>
            {
                e.ToTable("AnswerOptions");

                // Column constraints
                e.Property(a => a.Text)
                 .HasMaxLength(1000);

                e.Property(a => a.MediaKey)
                 .HasMaxLength(500);

                e.Property(a => a.MediaType)
                 .HasMaxLength(100);

                e.Property(a => a.Feedback)
                 .HasMaxLength(1000);

                e.HasOne(a => a.Question)
                 .WithMany(q => q.Options)
                 .HasForeignKey(a => a.QuestionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });



            // TeacherPackage
            modelBuilder.Entity<TeacherPackage>(e =>
            {
                e.ToTable("TeacherPackages");

                // Column constraints
                e.Property(tp => tp.PackageName)
                 .IsRequired()
                 .HasMaxLength(100);

                e.Property(tp => tp.Price)
                 .HasPrecision(18, 2);
            });

            // TeacherSubscription
            modelBuilder.Entity<TeacherSubscription>(e =>
            {
                e.ToTable("TeacherSubscriptions");
                e.HasOne(ts => ts.User)
                 .WithMany(u => u.TeacherSubscriptions)
                 .HasForeignKey(ts => ts.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(ts => ts.TeacherPackage)
                 .WithMany(tp => tp.Subscriptions)
                 .HasForeignKey(ts => ts.TeacherPackageId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(ts => ts.Payment)
                 .WithMany()
                 .HasForeignKey(ts => ts.PaymentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // RefreshToken
            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.ToTable("RefreshTokens");

                // Column constraints
                e.Property(rt => rt.Token)
                 .IsRequired()
                 .HasMaxLength(500);

                e.Property(rt => rt.ExpiresAt)
                 .IsRequired();

                e.Property(rt => rt.CreatedAt)
                 .IsRequired();

                e.Property(rt => rt.IsRevoked)
                 .IsRequired()
                 .HasDefaultValue(false);

                // Indexes
                e.HasIndex(rt => rt.Token);
                e.HasIndex(rt => new { rt.UserId, rt.IsRevoked });
                e.HasIndex(rt => rt.ExpiresAt);

                // Relationships
                e.HasOne(rt => rt.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(rt => rt.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // PasswordResetToken
            modelBuilder.Entity<PasswordResetToken>(e =>
            {
                e.ToTable("PasswordResetTokens");

                // Column constraints
                e.Property(prt => prt.Token)
                 .IsRequired()
                 .HasMaxLength(500);

                e.Property(prt => prt.ExpiresAt)
                 .IsRequired();

                e.Property(prt => prt.CreatedAt)
                 .IsRequired();

                e.Property(prt => prt.IsUsed)
                 .IsRequired()
                 .HasDefaultValue(false);

                // Indexes
                e.HasIndex(prt => prt.Token);
                e.HasIndex(prt => new { prt.UserId, prt.IsUsed });
                e.HasIndex(prt => prt.ExpiresAt);

                // Relationships
                e.HasOne(prt => prt.User)
                 .WithMany(u => u.PasswordResetTokens)
                 .HasForeignKey(prt => prt.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== EmailVerificationToken =====
            modelBuilder.Entity<EmailVerificationToken>(e =>
            {
                e.ToTable("EmailVerificationTokens");

                // Column constraints
                e.Property(evt => evt.Email)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(evt => evt.OtpCode)
                 .IsRequired()
                 .HasMaxLength(10);

                e.Property(evt => evt.CreatedAt)
                 .IsRequired();

                e.Property(evt => evt.ExpiresAt)
                 .IsRequired();

                e.Property(evt => evt.IsUsed)
                 .IsRequired()
                 .HasDefaultValue(false);

                // Indexes
                e.HasIndex(evt => evt.OtpCode);
                e.HasIndex(evt => new { evt.UserId, evt.IsUsed });
                e.HasIndex(evt => evt.ExpiresAt);
                e.HasIndex(evt => new { evt.Email, evt.OtpCode });

                // Relationships
                e.HasOne(evt => evt.User)
                 .WithMany()
                 .HasForeignKey(evt => evt.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== ExternalLogin =====
            modelBuilder.Entity<ExternalLogin>(e =>
            {
                e.ToTable("ExternalLogins");
                e.HasKey(el => el.ExternalLoginId);

                // Composite unique index: Prevent duplicate provider accounts
                // One Google account can only be linked to ONE user
                e.HasIndex(el => new { el.Provider, el.ProviderUserId })
                 .IsUnique()
                 .HasDatabaseName("IX_ExternalLogin_Provider_ProviderUserId");

                // Foreign key relationship
                e.HasOne(el => el.User)
                 .WithMany(u => u.ExternalLogins)
                 .HasForeignKey(el => el.UserId)
                 .OnDelete(DeleteBehavior.Cascade); // Delete external logins when user is deleted

                // Column constraints
                e.Property(el => el.Provider)
                 .IsRequired()
                 .HasMaxLength(50); // "Google", "Facebook", "Microsoft"

                e.Property(el => el.ProviderUserId)
                 .IsRequired()
                 .HasMaxLength(255); // Google's sub is long

                e.Property(el => el.ProviderDisplayName)
                 .HasMaxLength(255);

                e.Property(el => el.ProviderPhotoUrl)
                 .HasMaxLength(500); // URLs can be long

                e.Property(el => el.ProviderEmail)
                 .HasMaxLength(255);

                e.Property(el => el.CreatedAt)
                 .IsRequired();

                e.Property(el => el.LastUsedAt)
                 .IsRequired(false);
            });

            // QuizAttempt
            modelBuilder.Entity<QuizAttempt>(e =>
            {
                e.ToTable("QuizAttempts");
                e.HasKey(qa => qa.AttemptId);

                // Column constraints
                e.Property(qa => qa.TotalScore)
                 .HasPrecision(18, 2);

                e.Property(qa => qa.ScoresJson)
                 .HasMaxLength(10000);

                e.Property(qa => qa.AnswersJson)
                 .HasMaxLength(10000);

                e.HasOne(qa => qa.Quiz)
                 .WithMany(q => q.Attempts)
                 .HasForeignKey(qa => qa.QuizId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(qa => qa.User)
                 .WithMany(u => u.QuizAttempts)
                 .HasForeignKey(qa => qa.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });



            // EssaySubmission
            modelBuilder.Entity<EssaySubmission>(e =>
            {
                e.HasKey(es => es.SubmissionId);
                e.ToTable("EssaySubmissions");

                // Column constraints
                e.Property(es => es.TextContent)
                 .HasMaxLength(20000);

                e.Property(es => es.AttachmentKey)
                 .HasMaxLength(500);

                e.Property(es => es.AttachmentType)
                 .HasMaxLength(100);

                e.HasOne(es => es.Essay)
                 .WithMany(e => e.EssaySubmissions)
                 .HasForeignKey(es => es.EssayId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(es => es.User)
                 .WithMany(u => u.EssaySubmissions)
                 .HasForeignKey(es => es.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ModuleCompletion
            modelBuilder.Entity<ModuleCompletion>(e =>
            {
                e.ToTable("ModuleCompletions");
                e.HasOne(mc => mc.Module)
                 .WithMany(m => m.ModuleCompletions)
                 .HasForeignKey(mc => mc.ModuleId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(mc => mc.User)
                 .WithMany(u => u.ModuleCompletions)
                 .HasForeignKey(mc => mc.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // LessonCompletion
            modelBuilder.Entity<LessonCompletion>(e =>
            {
                e.ToTable("LessonCompletions");
                e.HasOne(lc => lc.Lesson)
                 .WithMany(l => l.LessonCompletions)
                 .HasForeignKey(lc => lc.LessonId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(lc => lc.User)
                 .WithMany(u => u.LessonCompletions)
                 .HasForeignKey(lc => lc.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // CourseProgress
            modelBuilder.Entity<CourseProgress>(e =>
            {
                e.ToTable("CourseProgresses");
                e.HasOne(cp => cp.User)
                 .WithMany(u => u.CourseProgresses)
                 .HasForeignKey(cp => cp.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(cp => cp.Course)
                 .WithMany(c => c.CourseProgresses)
                 .HasForeignKey(cp => cp.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // FlashCardReview
            modelBuilder.Entity<FlashCardReview>(e =>
            {
                e.ToTable("FlashCardReviews");
                e.HasOne(fcr => fcr.User)
                 .WithMany(u => u.FlashCardReviews)
                 .HasForeignKey(fcr => fcr.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(fcr => fcr.FlashCard)
                 .WithMany(fc => fc.Reviews)
                 .HasForeignKey(fcr => fcr.FlashCardId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // PronunciationProgress
            modelBuilder.Entity<PronunciationProgress>(e =>
            {
                e.ToTable("PronunciationProgresses");

                // Unique constraint: One progress record per User-FlashCard pair
                e.HasIndex(pp => new { pp.UserId, pp.FlashCardId })
                 .IsUnique();

                e.HasOne(pp => pp.User)
                 .WithMany(u => u.PronunciationProgresses)
                 .HasForeignKey(pp => pp.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(pp => pp.FlashCard)
                 .WithMany(fc => fc.PronunciationProgresses)
                 .HasForeignKey(pp => pp.FlashCardId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Streak
            modelBuilder.Entity<Streak>(e =>
            {
                e.ToTable("Streaks");
                e.HasOne(s => s.User)
                 .WithMany(u => u.Streaks)
                 .HasForeignKey(s => s.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Notification
            modelBuilder.Entity<Notification>(e =>
            {
                e.ToTable("Notifications");
                e.HasKey(n => n.Id);

                e.Property(n => n.Title)
                 .HasMaxLength(200)
                 .IsRequired(false);

                e.Property(n => n.Message)
                 .HasMaxLength(1000)
                 .IsRequired(false);

                e.Property(n => n.Type)
                 .IsRequired();

                e.Property(n => n.IsRead)
                 .HasDefaultValue(false);

                e.HasOne(n => n.User)
                 .WithMany(u => u.Notifications)
                 .HasForeignKey(n => n.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ActivityLog
            modelBuilder.Entity<ActivityLog>(e =>
            {
                e.ToTable("ActivityLogs");

                // Column constraints
                e.Property(al => al.Action)
                 .IsRequired()
                 .HasMaxLength(255);

                e.HasOne(al => al.User)
                 .WithMany(u => u.ActivityLogs)
                 .HasForeignKey(al => al.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // UserCourse
            modelBuilder.Entity<UserCourse>(e =>
            {
                e.ToTable("UserCourses");
                e.HasIndex(uc => new { uc.UserId, uc.CourseId }).IsUnique();
                e.HasOne(uc => uc.User)
                 .WithMany()
                 .HasForeignKey(uc => uc.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(uc => uc.Course)
                 .WithMany(c => c.UserCourses)
                 .HasForeignKey(uc => uc.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(uc => uc.Payment)
                 .WithMany()
                 .HasForeignKey(uc => uc.PaymentId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment
            modelBuilder.Entity<Payment>(e =>
            {
                e.ToTable("Payments");

                // Column constraints
                e.Property(p => p.PaymentMethod)
                 .HasMaxLength(50);

                e.Property(p => p.Amount)
                 .HasPrecision(18, 2);

                e.Property(p => p.ProviderTransactionId)
                 .HasMaxLength(255);

                e.HasOne(p => p.User)
                 .WithMany(u => u.Payments)
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Indexes - Essential for filtering
                e.HasIndex(p => new { p.UserId, p.Status });
                e.HasIndex(p => new { p.ProductType, p.ProductId });
            });



            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, Name = "Admin" },
                new Role { RoleId = 2, Name = "Teacher" },
                new Role { RoleId = 3, Name = "Student" }
            );

            // Dùng thời gian cố định để tránh thay đổi snapshot migration mỗi lần build
            var fixedCreated = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);

            // Admin user
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("05022004");
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    FirstName = "Admin",
                    LastName = "System",
                    Email = "minhxoandev@gmail.com",
                    NormalizedEmail = "MINHXOANDEV@GMAIL.COM",
                    PasswordHash = adminPasswordHash,
                    PhoneNumber = "0257554479",
                    DateOfBirth = new DateTime(2004, 2, 5),
                    IsMale = true,
                    EmailVerified = true,
                    AvatarKey = null,
                    CreatedAt = fixedCreated,
                    UpdatedAt = fixedCreated,
                    Status = AccountStatus.Active,
                    CurrentTeacherSubscriptionId = null
                }
            );

            // Gán role Admin cho user 1 (sử dụng junction table)
            modelBuilder.Entity("UserRoles").HasData(
                new { UserId = 1, RoleId = 1 }
            );
        }
    }
}
