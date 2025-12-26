namespace LearningEnglish.Domain.Enums;

public enum WebhookStatus
{
    Pending = 0,      // Webhook mới nhận, chưa xử lý
    Processing = 1,   // Đang xử lý
    Processed = 2,    // Xử lý thành công
    Failed = 3,       // Xử lý thất bại, sẽ retry
    DeadLetter = 4    // Đã retry max lần, chuyển sang dead letter queue
}
