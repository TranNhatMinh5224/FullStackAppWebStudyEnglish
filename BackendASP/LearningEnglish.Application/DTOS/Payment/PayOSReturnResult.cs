namespace LearningEnglish.Application.DTOs
{
    public class PayOSReturnResult
    {
        public bool Success { get; set; }
        public string? RedirectUrl { get; set; }
        public string? Message { get; set; }
        public int? PaymentId { get; set; }
        public string? OrderCode { get; set; }
    }
}