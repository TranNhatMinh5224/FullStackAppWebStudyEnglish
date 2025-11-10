namespace LearningEnglish.Application.Common.Utils
{
    public static class ClassCodeGenerator
    {
        // GUID 128-bit, lấy 8 ký tự đầu dạng hex, viết hoa
        public static string Generate()
        {
            return Guid.NewGuid().ToString("N")[..8].ToUpper();
        }
    }
}
