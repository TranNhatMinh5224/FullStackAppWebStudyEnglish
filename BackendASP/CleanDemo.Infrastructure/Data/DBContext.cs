using Microsoft.EntityFrameworkCore;
using CleanDemo.Domain.Entities;

namespace CleanDemo.Infrastructure.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }

  // DbSets
  public DbSet<User> Users { get; set; }
  public DbSet<Role> Roles { get; set; }
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
  public DbSet<UserRole> UserRoles { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // User Configuration
    modelBuilder.Entity<User>(entity =>
    {
      entity.ToTable("Users");
      entity.HasIndex(u => u.Email).IsUnique();

      // User-Role Many-to-Many (sử dụng entity UserRole)
      entity.HasMany(u => u.Roles)
                    .WithMany(r => r.Users)
                    .UsingEntity<UserRole>(
                        j => j.HasOne(ur => ur.Role).WithMany().HasForeignKey(ur => ur.RolesRoleId),
                        j => j.HasOne(ur => ur.User).WithMany().HasForeignKey(ur => ur.UsersUserId)
                    );
    });

    // Role Configuration
    modelBuilder.Entity<Role>(entity =>
    {
      entity.ToTable("Roles");
    });

    // UserRole Configuration
    modelBuilder.Entity<UserRole>(entity =>
    {
      entity.ToTable("UserRoles");
      entity.HasKey(ur => new { ur.UsersUserId, ur.RolesRoleId });
    });

    // Course Configuration
    modelBuilder.Entity<Course>(entity =>
    {
      entity.ToTable("Courses");

      entity.Property(c => c.Price)
            .HasPrecision(10, 2); // 10 digits total, 2 decimal places

      entity.HasOne(c => c.Teacher)
            .WithMany(u => u.CreatedCourses)
            .HasForeignKey(c => c.TeacherId)
            .IsRequired(false) // Cho phép null
            .OnDelete(DeleteBehavior.SetNull); // Set null khi xóa Teacher
    });

    // Lesson Configuration
    modelBuilder.Entity<Lesson>(entity =>
    {
      entity.ToTable("Lessons");

      // Lesson thuộc về Course (không còn Topic)
      entity.HasOne(l => l.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
    });

    // Vocabulary Configuration
    modelBuilder.Entity<Vocabulary>(entity =>
    {
      entity.ToTable("Vocabularies");

      entity.HasOne(v => v.Lesson)
                .WithMany(l => l.Vocabularies)
                .HasForeignKey(v => v.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
    });

    // MiniTest Configuration
    modelBuilder.Entity<MiniTest>(entity =>
    {
      entity.ToTable("MiniTests");

      entity.HasOne(mt => mt.Lesson)
                .WithMany(l => l.MiniTests)
                .HasForeignKey(mt => mt.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
    });

    // Question Configuration
    modelBuilder.Entity<Question>(entity =>
    {
      entity.ToTable("Questions");

      entity.HasOne(q => q.MiniTest)
                .WithMany(mt => mt.Questions)
                .HasForeignKey(q => q.MiniTestId)
                .OnDelete(DeleteBehavior.Cascade);
    });

    // AnswerOption Configuration
    modelBuilder.Entity<AnswerOption>(entity =>
    {
      entity.ToTable("AnswerOptions");

      entity.HasOne(ao => ao.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(ao => ao.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
    });

    // UserCourse Configuration
    modelBuilder.Entity<UserCourse>(entity =>
    {
      entity.ToTable("UserCourses");

      entity.HasOne(uc => uc.User)
                .WithMany(u => u.UserCourses)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(uc => uc.Course)
                .WithMany(c => c.UserCourses)
                .HasForeignKey(uc => uc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

      // Unique constraint: One user can only enroll in a course once
      entity.HasIndex(uc => new { uc.UserId, uc.CourseId }).IsUnique();
    });

    // TeacherPackage Configuration
    modelBuilder.Entity<TeacherPackage>(entity =>
    {
      entity.ToTable("TeacherPackages");
    });

    // TeacherSubscription Configuration
    modelBuilder.Entity<TeacherSubscription>(entity =>
    {
      entity.ToTable("TeacherSubscriptions");

      entity.HasOne(ts => ts.User)
                .WithMany()
                .HasForeignKey(ts => ts.UserId)
                .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(ts => ts.TeacherPackage)
                .WithMany()
                .HasForeignKey(ts => ts.TeacherPackageId)
                .OnDelete(DeleteBehavior.Restrict);
    });

    // RefreshToken Configuration
    modelBuilder.Entity<RefreshToken>(entity =>
    {
      entity.ToTable("RefreshTokens");

      entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
    });

    // PasswordResetToken Configuration
    modelBuilder.Entity<PasswordResetToken>(entity =>
    {
      entity.ToTable("PasswordResetTokens");

      entity.HasOne(prt => prt.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(prt => prt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
    });

    // Progress Configuration
    modelBuilder.Entity<Progress>(entity =>
    {
      entity.ToTable("Progress");

      entity.HasOne(p => p.User)
                .WithMany(u => u.ProgressRecords)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(p => p.Lesson)
                .WithMany(l => l.ProgressRecords)
                .HasForeignKey(p => p.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
    });

    // ReviewWord Configuration
    modelBuilder.Entity<ReviewWord>(entity =>
    {
      entity.ToTable("ReviewWords");

      entity.HasOne(rw => rw.User)
                .WithMany(u => u.ReviewWords)
                .HasForeignKey(rw => rw.UserId)
                .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(rw => rw.Vocabulary)
                .WithMany(v => v.ReviewWords)
                .HasForeignKey(rw => rw.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade);
    });

    // PronunciationScore Configuration
    modelBuilder.Entity<PronunciationScore>(entity =>
    {
      entity.ToTable("PronunciationScores");

      entity.HasOne(ps => ps.User)
                .WithMany(u => u.PronunciationScores)
                .HasForeignKey(ps => ps.UserId)
                .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(ps => ps.Vocabulary)
                .WithMany(v => v.PronunciationScores)
                .HasForeignKey(ps => ps.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade);
    });

    // Payment Configuration
    modelBuilder.Entity<Payment>(entity =>
    {
      entity.ToTable("Payments");

      entity.Property(p => p.Amount)
                .HasPrecision(10, 2); // 10 digits total, 2 decimal places

      entity.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(p => p.Course)
                .WithMany()
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.SetNull);
    });

    // Seed Data
    SeedData(modelBuilder);
  }

  private void SeedData(ModelBuilder modelBuilder)
  {
    // Seed Roles
    modelBuilder.Entity<Role>().HasData(
        new Role { RoleId = 1, Name = "Admin" },
        new Role { RoleId = 2, Name = "Teacher" },
        new Role { RoleId = 3, Name = "Student" }
    );

    // Seed Admin User
    var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456789"); // Thay đổi mật khẩu mặc định nếu cần
    modelBuilder.Entity<User>().HasData(
        new User
        {
          UserId = 1,
          FirstName = "Admin",
          LastName = "System",
          Email = "admin@example.com", // Thay đổi email nếu cần
          PasswordHash = adminPasswordHash,
          PhoneNumber = "",
          CreatedAt = DateTime.UtcNow,
          UpdatedAt = DateTime.UtcNow,
          Status = CleanDemo.Domain.Enums.StatusAccount.Active
        }
    );

    // Seed UserRoles for Admin
    modelBuilder.Entity<UserRole>().HasData(
        new UserRole { UsersUserId = 1, RolesRoleId = 1 } // Gán user ID 1 với role ID 1 (Admin)
    );
  }
}

