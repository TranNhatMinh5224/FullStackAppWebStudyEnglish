using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningEnglish.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLectureTypeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update LectureType enum values
            // Old enum: Content = 1, Video = 2, Audio = 3, Document = 4, Interactive = 5
            // New enum: Content = 1, Document = 2, Video = 3
            
            // Conversion mapping:
            // Content (1) -> Content (1) - giữ nguyên
            // Video (2) -> Video (3) - đổi từ 2 sang 3
            // Audio (3) -> Content (1) - đổi từ 3 sang 1
            // Document (4) -> Document (2) - đổi từ 4 sang 2
            // Interactive (5) -> Video (3) - đổi từ 5 sang 3
            
            // Step 1: Convert old Video (2) to temporary value (99) to avoid conflict
            migrationBuilder.Sql(@"
                UPDATE ""Lectures""
                SET ""Type"" = 99
                WHERE ""Type"" = 2;
            ");

            // Step 2: Convert old Document (4) to new Document (2) - safe, no conflict
            migrationBuilder.Sql(@"
                UPDATE ""Lectures""
                SET ""Type"" = 2
                WHERE ""Type"" = 4;
            ");

            // Step 3: Convert old Audio (3) to new Content (1) - safe, no conflict
            migrationBuilder.Sql(@"
                UPDATE ""Lectures""
                SET ""Type"" = 1
                WHERE ""Type"" = 3;
            ");

            // Step 4: Convert old Interactive (5) to new Video (3) - safe, no conflict
            migrationBuilder.Sql(@"
                UPDATE ""Lectures""
                SET ""Type"" = 3
                WHERE ""Type"" = 5;
            ");

            // Step 5: Convert temporary Video (99) to new Video (3)
            migrationBuilder.Sql(@"
                UPDATE ""Lectures""
                SET ""Type"" = 3
                WHERE ""Type"" = 99;
            ");

            // Step 6: Safety check - Convert any invalid types to Content (1) as default
            migrationBuilder.Sql(@"
                UPDATE ""Lectures""
                SET ""Type"" = 1
                WHERE ""Type"" NOT IN (1, 2, 3);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to old enum values
            // Old enum: Content = 1, Video = 2, Audio = 3, Document = 4, Interactive = 5
            // New enum: Content = 1, Document = 2, Video = 3

            // Step 1: Convert new Video (3) back to old Video (2)
            // Temporarily set to 99 to avoid conflict
            migrationBuilder.Sql(@"
                UPDATE ""Lectures""
                SET ""Type"" = 99
                WHERE ""Type"" = 3;
            ");

            // Step 2: Convert new Document (2) back to old Document (4)
            migrationBuilder.Sql(@"
                UPDATE ""Lectures""
                SET ""Type"" = 4
                WHERE ""Type"" = 2;
            ");

            // Step 3: Convert new Video (99) back to old Video (2)
            migrationBuilder.Sql(@"
                UPDATE ""Lectures""
                SET ""Type"" = 2
                WHERE ""Type"" = 99;
            ");

            // Note: We cannot perfectly restore Audio (3) and Interactive (5) 
            // as they were converted to Content (1) and Video (3) respectively
            // This is a data loss scenario, but necessary for enum simplification
        }
    }
}
