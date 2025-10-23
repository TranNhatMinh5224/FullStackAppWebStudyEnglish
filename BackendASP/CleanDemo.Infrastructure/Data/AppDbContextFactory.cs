using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using DotNetEnv;

namespace CleanDemo.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Load .env file
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Read PostgreSQL connection info from environment variables
            var dbServer = Environment.GetEnvironmentVariable("DB__Server_ASPELEARNING") ?? "localhost";
            var dbPort = Environment.GetEnvironmentVariable("DB__Port_ASPELEARNING") ?? "5432";
            var dbName = Environment.GetEnvironmentVariable("DB__Name_ASPELEARNING") ?? "Elearning";
            var dbUser = Environment.GetEnvironmentVariable("DB__User_ASPELEARNING") ?? "postgres";
            var dbPassword = Environment.GetEnvironmentVariable("DB__Password_ASPELEARNING") ?? "12122004";

            var connectionString = $"Host={dbServer};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};";

            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
