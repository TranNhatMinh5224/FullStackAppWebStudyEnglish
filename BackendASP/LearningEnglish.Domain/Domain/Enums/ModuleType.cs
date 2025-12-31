namespace LearningEnglish.Domain.Enums;

// Loại cấu trúc nội dung chính trong một Module
public enum ModuleType
{
    Lecture = 1,      // Bài học lý thuyết (Video, Audio, Văn bản)
    Quiz = 2,         // Bài kiểm tra (Trắc nghiệm)
    FlashCard = 3,    // Học từ vựng qua thẻ
    Video = 4,        // Video
    Reading = 5,      // Đọc hiểu
    Assessment = 6    // Bài kiểm tra/đánh giá (Quiz, Essay)
}