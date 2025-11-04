namespace CleanDemo.Application.DTOs;
// Dto request cho MiniTest
public class MiniTestDto
{
    public string Title { get; set; } = string.Empty;
    public int LessonId { get; set; }

}
// Dto response cho MiniTest
public class MiniTestResponseDto
{
    public int MiniTestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int LessonId { get; set; }

   
}