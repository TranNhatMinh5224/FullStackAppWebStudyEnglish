using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Infrastructure.Data
{
    // Factory để tạo AppDbContext tại design-time (migrations, scaffolding)
    // Dùng cho EF Core khi không chạy API
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        private const string API_PROJECT_NAME = "LearningEnglish.API";
        private const string CONNECTION_STRING_KEY = "DefaultConnection";
        private const string APPSETTINGS_FILE = "appsettings.json";
        private const string APPSETTINGS_DEV_FILE = "appsettings.Development.json";

        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var connectionString = GetConnectionString(configuration);
            var options = BuildDbContextOptions(connectionString);

            return new AppDbContext(options);
        }

        // Build configuration từ appsettings.json
        private IConfiguration BuildConfiguration()
        {
            var basePath = GetConfigurationBasePath();

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(APPSETTINGS_FILE, optional: false, reloadOnChange: false)
                .AddJsonFile(APPSETTINGS_DEV_FILE, optional: true, reloadOnChange: false);

            return builder.Build();
        }

        // Xác định thư mục chứa appsettings (API project)
        private string GetConfigurationBasePath()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            // Chạy từ Infrastructure project
            if (currentDirectory.Contains("LearningEnglish.Infrastructure"))
            {
                return Path.Combine(currentDirectory, "..", API_PROJECT_NAME);
            }

            // Chạy từ root hoặc API project
            var apiPath = Path.Combine(currentDirectory, API_PROJECT_NAME);
            if (Directory.Exists(apiPath))
            {
                return apiPath;
            }

            // Fallback: tìm trong parent directory
            var parentApiPath = Path.Combine(currentDirectory, "..", API_PROJECT_NAME);
            if (Directory.Exists(parentApiPath))
            {
                return parentApiPath;
            }

            throw new DirectoryNotFoundException(
                $"Không tìm thấy thư mục {API_PROJECT_NAME}. " +
                $"Current directory: {currentDirectory}");
        }

        // Lấy connection string từ configuration
        private string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(CONNECTION_STRING_KEY);

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            // Fallback: build connection string thủ công
            return BuildConnectionStringFromSettings(configuration);
        }

        // Build connection string từ các cấu hình riêng lẻ
        private string BuildConnectionStringFromSettings(IConfiguration configuration)
        {
            var server = configuration["Database:Server"] ?? "localhost";
            var port = configuration["Database:Port"] ?? "5432";
            var database = configuration["Database:Name"] ?? "Elearning";
            var username = configuration["Database:User"] ?? "postgres";
            var password = configuration["Database:Password"] ?? string.Empty;

            return $"Host={server};Port={port};Database={database};Username={username};Password={password};";
        }

        // Build DbContextOptions
        private DbContextOptions<AppDbContext> BuildDbContextOptions(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string không được để trống. " +
                    "Vui lòng cấu hình trong appsettings.json hoặc environment variables.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return optionsBuilder.Options;
        }
    }
}
