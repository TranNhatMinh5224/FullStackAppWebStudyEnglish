using LearningEnglish.Application.Interface.Infrastructure;
using LearningEnglish.Infrastructure.Common.Constants;

namespace LearningEnglish.Infrastructure.Services;

/// <summary>
/// Implementation của IStorageConfigProvider
/// Cung cấp bucket names từ StorageConstants
/// </summary>
public class StorageConfigProvider : IStorageConfigProvider
{
    // Danh sách tất cả buckets cần cleanup temp files
    private static readonly string[] _bucketsForCleanup = new[]
    {
        StorageConstants.CourseImageBucket,      // courses
        StorageConstants.LessonImageBucket,      // lessons
        StorageConstants.LectureMediaBucket,     // lectures
        StorageConstants.QuizGroupBucket,        // quizgroups
        StorageConstants.QuestionBucket,         // questions
        StorageConstants.FlashCardBucket,        // flashcards
        StorageConstants.FlashCardAudioBucket,   // flashcard-audio
        StorageConstants.ModuleImageBucket,      // modules
        StorageConstants.AvatarBucket,           // avatars
        StorageConstants.EssayAttachmentBucket,  // essay-attachments
        StorageConstants.AssetImageBucket,       // assetsfrontend
        StorageConstants.PronunciationBucket     // pronunciations
    };

    public IReadOnlyList<string> GetAllBucketsForCleanup()
    {
        return _bucketsForCleanup;
    }
}
