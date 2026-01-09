namespace LearningEnglish.Domain.Enums;

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Expired = 4  // Payment link hết hạn (ExpiredAt)
}
