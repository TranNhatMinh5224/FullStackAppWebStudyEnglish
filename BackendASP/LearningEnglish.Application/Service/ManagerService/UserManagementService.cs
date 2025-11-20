using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Enums;
using AutoMapper;

namespace LearningEnglish.Application.Service
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;

        public UserManagementService(IUserRepository userRepository, IMapper mapper, ICourseRepository courseRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _courseRepository = courseRepository;
        }

        public async Task<ServiceResponse<UserDto>> GetUserProfileAsync(int userId)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy người dùng";
                    return response;
                }

                response.StatusCode = 200;
                response.Data = _mapper.Map<UserDto>(user);
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        public async Task<ServiceResponse<UserDto>> UpdateUserProfileAsync(int userId, UpdateUserDto dto)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy người dùng";
                    return response;
                }

                _mapper.Map(dto, user);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Message = "Cập nhật hồ sơ thành công";
                response.Data = _mapper.Map<UserDto>(user);
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        public async Task<ServiceResponse<List<UserDto>>> GetAllUsersAsync()
        {
            var response = new ServiceResponse<List<UserDto>>();
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                response.StatusCode = 200;
                response.Data = _mapper.Map<List<UserDto>>(users);
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // Block account
        public async Task<ServiceResponse<BlockAccountResponseDto>> BlockAccountAsync(int userId)
        {
            var response = new ServiceResponse<BlockAccountResponseDto>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy tài khoản người dùng";
                    return response;
                }

                var isAdmin = await _userRepository.GetUserRolesAsync(userId);
                if (isAdmin)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không thể block tài khoản Admin";
                    return response;
                }

                if (user.Status == AccountStatus.Inactive)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Tài khoản đã bị khóa trước đó";
                    return response;
                }

                user.Status = AccountStatus.Inactive;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Success = true;
                response.Data = new BlockAccountResponseDto { Message = "Block tài khoản thành công" };
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // Unblock account
        public async Task<ServiceResponse<UnblockAccountResponseDto>> UnblockAccountAsync(int userId)
        {
            var response = new ServiceResponse<UnblockAccountResponseDto>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy tài khoản người dùng";
                    return response;
                }

                if (user.Status == AccountStatus.Active)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Tài khoản hiện không bị khóa";
                    return response;
                }

                user.Status = AccountStatus.Active;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Success = true;
                response.Data = new UnblockAccountResponseDto { Message = "Unblock tài khoản thành công" };
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // Danh sách tài khoản bị khóa
        public async Task<ServiceResponse<List<UserDto>>> GetListBlockedAccountsAsync()
        {
            var response = new ServiceResponse<List<UserDto>>();
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                var blocked = users.Where(u => u.Status == AccountStatus.Inactive).ToList();
                response.StatusCode = 200;
                response.Success = true;
                response.Data = _mapper.Map<List<UserDto>>(blocked);
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // === Giữ từ feature/LVE-107-GetUserbyCourseId ===
        // Lấy danh sách người dùng theo id khóa học, có kiểm tra quyền truy cập
        public async Task<ServiceResponse<List<UserDto>>> GetUsersByCourseIdAsync(int courseId, int userId, string checkRole)
        {
            var response = new ServiceResponse<List<UserDto>>();
            try
            {
                var course = await _courseRepository.GetByIdAsync(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isAuthorized = false;
                if (checkRole == "Admin")
                {
                    isAuthorized = true;
                }
                else if (checkRole == "Teacher")
                {
                    if (course.TeacherId == userId)
                    {
                        isAuthorized = true;
                    }
                }

                if (!isAuthorized)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn chỉ được xem danh sách học sinh trong khóa học của mình";
                    return response;
                }

                var users = await _courseRepository.GetEnrolledUsers(courseId);
                response.Data = _mapper.Map<List<UserDto>>(users);
                response.StatusCode = 200;
                response.Success = true;
                response.Message = "Lấy danh sách học sinh thành công";
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // === Giữ từ dev ===
        // Lấy danh sách giáo viên
        public async Task<ServiceResponse<List<UserDto>>> GetListTeachersAsync()
        {
            var response = new ServiceResponse<List<UserDto>>();
            try
            {
                var teachers = await _userRepository.GetAllTeachersAsync();
                response.StatusCode = 200;
                response.Success = true;
                response.Data = _mapper.Map<List<UserDto>>(teachers);
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }
        // Implement lấy danh sách học sinh theo all course
        public async Task<ServiceResponse<List<StudentsByAllCoursesDto>>> GetStudentsByAllCoursesAsync()
        {
            var response = new ServiceResponse<List<StudentsByAllCoursesDto>>();
            try
            {
                var allCourses = await _courseRepository.GetAllCourses();
                var studentsByAllCourses = new List<StudentsByAllCoursesDto>();

                foreach (var course in allCourses)
                {
                    var usersInCourse = await _courseRepository.GetEnrolledUsers(course.CourseId);

                    var courseWithUsers = new StudentsByAllCoursesDto
                    {
                        CourseId = course.CourseId,
                        Title = course.Title,
                        Description = course.Description ?? "",
                        TeacherName = course.Teacher != null ?
                            $"{course.Teacher.FirstName} {course.Teacher.LastName}" : "System",
                        TotalUsers = usersInCourse.Count(),
                        Users = _mapper.Map<List<UserDto>>(usersInCourse)
                    };

                    studentsByAllCourses.Add(courseWithUsers);
                }

                response.Data = studentsByAllCourses;
                response.StatusCode = 200;
                response.Success = true;
                response.Message = "Lấy danh sách học sinh theo tất cả khóa học thành công";
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }
    }
}
