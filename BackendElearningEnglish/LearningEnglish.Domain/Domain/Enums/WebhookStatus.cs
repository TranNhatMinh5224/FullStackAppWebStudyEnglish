namespace LearningEnglish.Domain.Enums;

public enum WebhookStatus
{
    Pending = 1,      // Webhook mới nhận, chưa xử lý
    Processing = 2,   // Đang xử lý
    Processed = 3,    // Xử lý thành công
    Failed = 4,       // Xử lý thất bại, sẽ retry
    DeadLetter = 5    // Đã retry max lần, chuyển sang dead letter queue
}
