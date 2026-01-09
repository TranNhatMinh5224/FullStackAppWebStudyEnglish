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
        
        // Teacher lấy danh sách học viên trong course của mình
        Task<ServiceResponse<PagedResult<UserDto>>> GetUsersByCourseIdPagedAsync(int courseId, int teacherId, PageRequest request);
        
        // Teacher lấy chi tiết học sinh trong course của mình
        Task<ServiceResponse<StudentDetailInCourseDto>> GetStudentDetailInCourseAsync(int courseId, int studentId, int teacherId);
        
        // Thêm học sinh vào course
       
        Task<ServiceResponse<bool>> AddStudentToCourseByEmailAsync(int courseId, string studentEmail, int currentUserId);
        
        // Xóa học sinh khỏi course
        
        Task<ServiceResponse<bool>> RemoveStudentFromCourseAsync(int courseId, int studentId, int currentUserId);

        
    }
}
