namespace LearningEnglish.Domain.Enums;

public enum LectureType
{
    Content = 1,      // Bài đọc văn bản (Markdown/HTML)
    Video = 2,        // Bài giảng video
    Audio = 3,        // Bài nghe
    Document = 4,     // Tài liệu đính kèm (PDF, Docx...)
    Interactive = 5   // Bài giảng tương tác (Slide, SCORM...)
}
