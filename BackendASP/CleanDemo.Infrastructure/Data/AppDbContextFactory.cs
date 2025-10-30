using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CleanDemo.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Load appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "CleanDemo.API"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Read connection string from appsettings.json
            var connectionString = configuration.GetConnectionString("MyConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback: build from individual settings
                var dbServer = configuration["Database:Server"] ?? "localhost";
                var dbPort = configuration["Database:Port"] ?? "5432";
                var dbName = configuration["Database:Name"] ?? "Elearning";
                var dbUser = configuration["Database:User"] ?? "postgres";
                var dbPassword = configuration["Database:Password"] ?? "";

                connectionString = $"Host={dbServer};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};";
            }

            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
