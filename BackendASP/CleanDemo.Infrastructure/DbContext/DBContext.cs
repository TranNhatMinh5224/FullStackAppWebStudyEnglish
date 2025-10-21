using Microsoft.EntityFrameworkCore;
using CleanDemo.Domain.Entities;

namespace CleanDemo.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Vocabulary> Vocabularies { get; set; }
        public DbSet<MiniTest> MiniTests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<TeacherPackage> TeacherPackages { get; set; }
        public DbSet<TeacherSubscription> TeacherSubscriptions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Progress> ProgressRecords { get; set; }
        public DbSet<ReviewWord> ReviewWords { get; set; }
        public DbSet<PronunciationScore> PronunciationScores { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasIndex(u => u.Email).IsUnique();

                // User - UserRole (1-n)
                e.HasMany(u => u.UserRoles)
                 .WithOne(ur => ur.User)
                 .HasForeignKey(ur => ur.UsersUserId)
                 .OnDelete(DeleteBehavior.Cascade);

                // CurrentTeacherSubscription (nullable)
                e.HasOne(u => u.CurrentTeacherSubscription)
                 .WithMany()
                 .HasForeignKey(u => u.CurrentTeacherSubscriptionId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // Role
            modelBuilder.Entity<Role>(e =>
            {
                e.ToTable("Roles");
                e.HasIndex(r => r.Name).IsUnique(); // tránh trùng tên role

                // Role - UserRole (1-n)
                e.HasMany(r => r.UserRoles)
                 .WithOne(ur => ur.Role)
                 .HasForeignKey(ur => ur.RolesRoleId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // UserRole (join entity)
            modelBuilder.Entity<UserRole>(e =>
            {
                e.ToTable("UserRoles");
                e.HasKey(ur => new { ur.UsersUserId, ur.RolesRoleId });
            });

            // Course
            modelBuilder.Entity<Course>(e =>
            {
                e.ToTable("Courses");

                // tiền tệ
                e.Property(c => c.Price).HasPrecision(18, 2);

                // Course -> Teacher (nullable)
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

            // Vocabulary
            modelBuilder.Entity<Vocabulary>(e =>
            {
                e.ToTable("Vocabularies");
                e.HasOne(v => v.Lesson)
                 .WithMany(l => l.Vocabularies)
                 .HasForeignKey(v => v.LessonId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // MiniTest
            modelBuilder.Entity<MiniTest>(e =>
            {
                e.ToTable("MiniTests");
                e.HasOne(mt => mt.Lesson)
                 .WithMany(l => l.MiniTests)
                 .HasForeignKey(mt => mt.LessonId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Question
            modelBuilder.Entity<Question>(e =>
            {
                e.ToTable("Questions");
                e.HasOne(q => q.MiniTest)
                 .WithMany(mt => mt.Questions)
                 .HasForeignKey(q => q.MiniTestId)
                 .OnDelete(DeleteBehavior.Cascade);
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

            // UserCourse
            modelBuilder.Entity<UserCourse>(e =>
            {
                e.ToTable("UserCourses");

                // mỗi user chỉ tham gia 1 lần / 1 course
                e.HasIndex(uc => new { uc.UserId, uc.CourseId }).IsUnique();

                e.HasOne(uc => uc.User)
                 .WithMany(u => u.UserCourses)
                 .HasForeignKey(uc => uc.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(uc => uc.Course)
                 .WithMany(c => c.UserCourses)
                 .HasForeignKey(uc => uc.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

                // link tới Payment (nullable)
                e.HasOne(uc => uc.Payment)
                 .WithMany()
                 .HasForeignKey(uc => uc.PaymentId)
                 .OnDelete(DeleteBehavior.Restrict);
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

                // link tới Payment (nullable)
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

            // Progress
            modelBuilder.Entity<Progress>(e =>
            {
                e.ToTable("Progress");
                e.HasOne(p => p.User)
                 .WithMany(u => u.ProgressRecords)
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(p => p.Lesson)
                 .WithMany(l => l.ProgressRecords)
                 .HasForeignKey(p => p.LessonId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ReviewWord
            modelBuilder.Entity<ReviewWord>(e =>
            {
                e.ToTable("ReviewWords");
                e.HasOne(rw => rw.User)
                 .WithMany(u => u.ReviewWords)
                 .HasForeignKey(rw => rw.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(rw => rw.Vocabulary)
                 .WithMany(v => v.ReviewWords)
                 .HasForeignKey(rw => rw.VocabularyId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // PronunciationScore
            modelBuilder.Entity<PronunciationScore>(e =>
            {
                e.ToTable("PronunciationScores");
                e.HasOne(ps => ps.User)
                 .WithMany(u => u.PronunciationScores)
                 .HasForeignKey(ps => ps.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(ps => ps.Vocabulary)
                 .WithMany(v => v.PronunciationScores)
                 .HasForeignKey(ps => ps.VocabularyId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment
            modelBuilder.Entity<Payment>(e =>
            {
                e.ToTable("Payments");

                // tiền tệ
                e.Property(p => p.Amount).HasPrecision(18, 2);

                e.HasOne(p => p.User)
                 .WithMany(u => u.Payments)
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                // index hỗ trợ truy vấn
                e.HasIndex(p => new { p.UserId, p.Status });
                e.HasIndex(p => new { p.ProductType, p.ProductId });
            });

            // Seed
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

            // Admin user (chú ý: seed thời gian bằng giá trị cố định nếu bạn dùng migration snapshot)
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = CleanDemo.Domain.Enums.StatusAccount.Active
                }
            );

            // Gán role Admin cho user 1
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UsersUserId = 1, RolesRoleId = 1 }
            );
        }
    }
}
