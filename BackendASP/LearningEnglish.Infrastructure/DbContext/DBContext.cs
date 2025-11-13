using Microsoft.EntityFrameworkCore;
using LearningEnglish.Domain.Entities;

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
        public DbSet<QuizAttemptResult> QuizAttemptResults => Set<QuizAttemptResult>();
        public DbSet<QuizUserAnswer> QuizUserAnswers => Set<QuizUserAnswer>();
        public DbSet<QuizUserAnswerOption> QuizUserAnswerOptions => Set<QuizUserAnswerOption>();
        public DbSet<EssaySubmission> EssaySubmissions => Set<EssaySubmission>();
        public DbSet<UserCourse> UserCourses => Set<UserCourse>();
        public DbSet<TeacherPackage> TeacherPackages => Set<TeacherPackage>();
        public DbSet<TeacherSubscription> TeacherSubscriptions => Set<TeacherSubscription>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<ModuleCompletion> ModuleCompletions => Set<ModuleCompletion>();
        public DbSet<LessonCompletion> LessonCompletions => Set<LessonCompletion>();
        public DbSet<CourseProgress> CourseProgresses => Set<CourseProgress>();
        public DbSet<FlashCardReview> FlashCardReviews => Set<FlashCardReview>();
        public DbSet<PronunciationAssessment> PronunciationAssessments => Set<PronunciationAssessment>();
        public DbSet<Streak> Streaks => Set<Streak>();
        public DbSet<StudyReminder> StudyReminders => Set<StudyReminder>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== User =====
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasIndex(u => u.Email).IsUnique();

                // CurrentTeacherSubscription configuration removed
                // since CurrentTeacherSubscription property is not in User entity
            });

            // ===== Role =====
            modelBuilder.Entity<Role>(e =>
            {
                e.ToTable("Roles");
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
                e.Property(c => c.Price).HasPrecision(18, 2);
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
                e.HasOne(l => l.Course)
                 .WithMany(c => c.Lessons)
                 .HasForeignKey(l => l.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Module
            modelBuilder.Entity<Module>(e =>
            {
                e.ToTable("Modules");
                e.HasOne(m => m.Lesson)
                 .WithMany(l => l.Modules)
                 .HasForeignKey(m => m.LessonId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Lecture
            modelBuilder.Entity<Lecture>(e =>
            {
                e.ToTable("Lectures");
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
                e.Property(a => a.TotalPoints).HasPrecision(18, 2);
                e.HasOne(a => a.Module)
                 .WithMany(m => m.Assessments)
                 .HasForeignKey(a => a.ModuleId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Quiz
            modelBuilder.Entity<Quiz>(e =>
            {
                e.ToTable("Quizzes");
                e.HasOne(q => q.Assessment)
                 .WithMany(a => a.Quizzes)
                 .HasForeignKey(q => q.AssessmentId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // QuizSection
            modelBuilder.Entity<QuizSection>(e =>
            {
                e.ToTable("QuizSections");
                e.HasOne(qs => qs.Quiz)
                 .WithMany(q => q.QuizSections)
                 .HasForeignKey(qs => qs.QuizId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // QuizGroup
            modelBuilder.Entity<QuizGroup>(e =>
            {
                e.ToTable("QuizGroups");
                e.HasOne(qg => qg.QuizSection)
                 .WithMany(qs => qs.QuizGroups)
                 .HasForeignKey(qg => qg.QuizSectionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Essay
            modelBuilder.Entity<Essay>(e =>
            {
                e.ToTable("Essays");
                e.HasOne(es => es.Assessment)
                 .WithMany(a => a.Essays)
                 .HasForeignKey(es => es.AssessmentId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Question
            modelBuilder.Entity<Question>(e =>
            {
                e.ToTable("Questions");
                e.HasOne(q => q.QuizSection)
                 .WithMany()
                 .HasForeignKey(q => q.QuizSectionId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
                e.HasOne(q => q.QuizGroup)
                 .WithMany(qg => qg.Questions)
                 .HasForeignKey(q => q.QuizGroupId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // AnswerOption
            modelBuilder.Entity<AnswerOption>(e =>
            {
                e.ToTable("AnswerOptions");
                e.HasOne(a => a.Question)
                 .WithMany(q => q.Options)
                 .HasForeignKey(a => a.QuestionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });



            // TeacherPackage
            modelBuilder.Entity<TeacherPackage>(e =>
            {
                e.ToTable("TeacherPackages");
                e.Property(tp => tp.Price).HasPrecision(18, 2);
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
                e.HasOne(rt => rt.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(rt => rt.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // PasswordResetToken
            modelBuilder.Entity<PasswordResetToken>(e =>
            {
                e.ToTable("PasswordResetTokens");
                e.HasOne(prt => prt.User)
                 .WithMany(u => u.PasswordResetTokens)
                 .HasForeignKey(prt => prt.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // QuizAttempt
            modelBuilder.Entity<QuizAttempt>(e =>
            {
                e.HasKey(qa => qa.AttemptId);
                e.ToTable("QuizAttempts");
                e.HasOne(qa => qa.Quiz)
                 .WithMany(q => q.Attempts)
                 .HasForeignKey(qa => qa.QuizId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(qa => qa.User)
                 .WithMany(u => u.QuizAttempts)
                 .HasForeignKey(qa => qa.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // QuizAttemptResult
            modelBuilder.Entity<QuizAttemptResult>(e =>
            {
                e.HasKey(qar => qar.ResultId);
                e.ToTable("QuizAttemptResults");
                e.Property(qar => qar.Score).HasPrecision(18, 2);
                e.Property(qar => qar.MaxScore).HasPrecision(18, 2);
                e.Property(qar => qar.Percentage).HasPrecision(5, 2);
                e.Property(qar => qar.ManualScore).HasPrecision(18, 2);
                
                // Không config navigation - để entities độc lập
                // Chỉ có FK AttemptId, ReviewedBy
            });

            // QuizUserAnswer
            modelBuilder.Entity<QuizUserAnswer>(e =>
            {
                e.ToTable("QuizUserAnswers");
                e.HasOne(qua => qua.QuizAttempt)
                 .WithMany(qa => qa.Answers)
                 .HasForeignKey(qua => qua.QuizAttemptId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(qua => qua.User)
                 .WithMany(u => u.QuizUserAnswers)
                 .HasForeignKey(qua => qua.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(qua => qua.Question)
                 .WithMany(q => q.UserAnswers)
                 .HasForeignKey(qua => qua.QuestionId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(qua => qua.SelectedOption)
                 .WithMany() 
                 .HasForeignKey(qua => qua.SelectedOptionId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // QuizUserAnswerOption
            modelBuilder.Entity<QuizUserAnswerOption>(e =>
            {
                e.ToTable("QuizUserAnswerOptions");
                e.HasKey(quao => new { quao.QuizUserAnswerId, quao.AnswerOptionId });
                e.HasOne(quao => quao.QuizUserAnswer)
                 .WithMany(qua => qua.SelectedOptions)
                 .HasForeignKey(quao => quao.QuizUserAnswerId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(quao => quao.AnswerOption)
                 .WithMany(ao => ao.UserAnswerOptions)
                 .HasForeignKey(quao => quao.AnswerOptionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // EssaySubmission
            modelBuilder.Entity<EssaySubmission>(e =>
            {
                e.HasKey(es => es.SubmissionId);
                e.ToTable("EssaySubmissions");
                e.HasOne(es => es.Assessment)
                 .WithMany(a => a.EssaySubmissions)
                 .HasForeignKey(es => es.AssessmentId)
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

            // PronunciationAssessment
            modelBuilder.Entity<PronunciationAssessment>(e =>
            {
                e.ToTable("PronunciationAssessments");
                e.HasOne(pa => pa.User)
                 .WithMany(u => u.PronunciationAssessments)
                 .HasForeignKey(pa => pa.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(pa => pa.FlashCard)
                 .WithMany(fc => fc.PronunciationAssessments)
                 .HasForeignKey(pa => pa.FlashCardId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(pa => pa.Assignment)
                 .WithMany(a => a.PronunciationAssessments)
                 .HasForeignKey(pa => pa.AssignmentId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
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

            // StudyReminder
            modelBuilder.Entity<StudyReminder>(e =>
            {
                e.ToTable("StudyReminders");
                e.HasOne(sr => sr.User)
                 .WithMany(u => u.StudyReminders)
                 .HasForeignKey(sr => sr.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ActivityLog
            modelBuilder.Entity<ActivityLog>(e =>
            {
                e.ToTable("ActivityLogs");
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
                e.Property(p => p.Amount).HasPrecision(18, 2);
                e.HasOne(p => p.User)
                 .WithMany(u => u.Payments)
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(p => new { p.UserId, p.Status });
                e.HasIndex(p => new { p.ProductType, p.ProductId });
            });



            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
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
                    PasswordHash = adminPasswordHash,
                    PhoneNumber = "0257554479",
                    CreatedAt = fixedCreated,
                    UpdatedAt = fixedCreated,
                    Status = LearningEnglish.Domain.Enums.AccountStatus.Active
                }
            );

            // Gán role Admin cho user 1 (sử dụng junction table)
            modelBuilder.Entity("UserRoles").HasData(
                new { UserId = 1, RoleId = 1 }
            );
        }
    }
}
