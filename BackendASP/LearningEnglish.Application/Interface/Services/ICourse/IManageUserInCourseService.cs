using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface IManageUserInCourseService
    {
        // Lấy danh sách học viên trong Course 
        
        Task<ServiceResponse<PagedResult<UserDto>>> GetUsersByCourseIdPagedAsync(int courseId, PageRequest request);
        
        // Lấy chi tiết học sinh trong course
     
        Task<ServiceResponse<StudentDetailInCourseDto>> GetStudentDetailInCourseAsync(int courseId, int studentId);
        
        // Thêm học sinh vào course
       
        Task<ServiceResponse<bool>> AddStudentToCourseByEmailAsync(int courseId, string studentEmail, int currentUserId);
        
        // Xóa học sinh khỏi course
        
        Task<ServiceResponse<bool>> RemoveStudentFromCourseAsync(int courseId, int studentId, int currentUserId);

        
    }
}
