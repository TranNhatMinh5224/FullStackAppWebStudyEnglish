namespace LearningEnglish.Application.Common
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; } = true;
        public int StatusCode { get; set; } = 200;
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
