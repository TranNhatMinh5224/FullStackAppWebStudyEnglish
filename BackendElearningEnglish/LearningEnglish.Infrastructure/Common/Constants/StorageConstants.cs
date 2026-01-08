namespace LearningEnglish.Infrastructure.Common.Constants;

/// <summary>
/// Centralized storage bucket and folder constants
/// Infrastructure-level constants for file storage configuration
/// Prevents hardcoded values scattered across services (DRY principle)
/// </summary>
public static class StorageConstants
{
    // Course Image Storage
    public const string CourseImageBucket = "courses";
    public const string CourseImageFolder = "real";

    // Lecture Media Storage
    public const string LectureMediaBucket = "lectures";
    public const string LectureMediaFolder = "real";

    // Lesson Image Storage
    public const string LessonImageBucket = "lessons";
    public const string LessonImageFolder = "real";

    // Module Image Storage
    public const string ModuleImageBucket = "modules";
    public const string ModuleImageFolder = "real";

    // Essay Storage
    public const string EssayAttachmentBucket = "essay-attachments";
    public const string EssayAttachmentFolder = "real";
    public const string EssayAudioBucket = "essays";
    public const string EssayAudioFolder = "audios";
    public const string EssayImageBucket = "essays";
    public const string EssayImageFolder = "images";

    // FlashCard Storage
    public const string FlashCardBucket = "flashcards";
    public const string FlashCardFolder = "real";

    // Avatar Storage
    public const string AvatarBucket = "avatars";
    public const string AvatarFolder = "real";

    // Quiz Storage
    public const string QuestionBucket = "questions";
    public const string QuestionFolder = "real";
    public const string QuizGroupBucket = "quizgroups";
    public const string QuizGroupFolder = "real";

    // Asset Frontend Storage
    public const string AssetImageBucket = "assetsfrontend";
    public const string AssetImageFolder = "real";
}

