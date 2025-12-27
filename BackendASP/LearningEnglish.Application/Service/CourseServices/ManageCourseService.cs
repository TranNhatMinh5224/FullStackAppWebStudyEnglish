using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Common.Pagination;
using AutoMapper;
using Microsoft.Extensions.Logging;


namespace LearningEnglish.Application.Service
{
    public class ManageUserInCourseService : IManageUserInCourseService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseProgressRepository _courseProgressRepository;
        private readonly ILogger<ManageUserInCourseService> _logger;

        // Bucket + folder cho avatar người dùng
        private const string AvatarBucket = "avatars";
        private const string AvatarFolder = "real";

        public ManageUserInCourseService(
            IUserRepository userRepository,
            IMapper mapper,
            ICourseRepository courseRepository,
            ICourseProgressRepository courseProgressRepository,
            ILogger<ManageUserInCourseService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _courseRepository = courseRepository;
            _courseProgressRepository = courseProgressRepository;
            _logger = logger;
        }


        public async Task<ServiceResponse<PagedResult<UserDto>>> GetUsersByCourseIdPagedAsync(int courseId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<UserDto>>();
            try
            {
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học hoặc bạn không có quyền truy cập";
                    return response;
                }

                var userParams = new UserQueryParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
                var pagedUsers = await _userRepository.GetUsersByCourseIdPagedAsync(courseId, userParams);

                var userDtos = _mapper.Map<List<UserDto>>(pagedUsers.Items);

                var result = new PagedResult<UserDto>
                {
                    Items = userDtos,
                    TotalCount = pagedUsers.TotalCount,
                    PageNumber = pagedUsers.PageNumber,
                    PageSize = pagedUsers.PageSize
                };

                response.Data = result;
                response.StatusCode = 200;
                response.Success = true;
                response.Message = "Lấy danh sách học sinh thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsersByCourseIdPagedAsync for CourseId: {CourseId}", courseId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // Teacher lấy danh sách học viên trong course của mình
        public async Task<ServiceResponse<PagedResult<UserDto>>> GetUsersByCourseIdPagedAsync(int courseId, int teacherId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<UserDto>>();
            try
            {
                var userParams = new UserQueryParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
                var pagedUsers = await _userRepository.GetUsersByCourseIdPagedForTeacherAsync(courseId, teacherId, userParams);

                var userDtos = _mapper.Map<List<UserDto>>(pagedUsers.Items);

                var result = new PagedResult<UserDto>
                {
                    Items = userDtos,
                    TotalCount = pagedUsers.TotalCount,
                    PageNumber = pagedUsers.PageNumber,
                    PageSize = pagedUsers.PageSize
                };

                response.Data = result;
                response.StatusCode = 200;
                response.Success = true;
                response.Message = "Lấy danh sách học sinh thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsersByCourseIdPagedAsync for CourseId: {CourseId}, TeacherId: {TeacherId}", courseId, teacherId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }


        // Lấy thông tin chi tiết của học sinh trong một course cụ thể

        public async Task<ServiceResponse<StudentDetailInCourseDto>> GetStudentDetailInCourseAsync(
            int courseId, 
            int studentId)
        {
            var response = new ServiceResponse<StudentDetailInCourseDto>();
            try
            {
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học hoặc bạn không có quyền truy cập";
                    return response;
                }

                // Lấy thông tin học sinh
                var student = await _userRepository.GetByIdAsync(studentId);
                if (student == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy học sinh";
                    return response;
                }

                // Kiểm tra học sinh có enrolled trong course này không
                var userCourse = await _courseRepository.GetUserCourseAsync(studentId, courseId);
                if (userCourse == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Học sinh chưa tham gia khóa học này";
                    return response;
                }

                // Lấy thông tin tiến độ học tập
                var courseProgress = await _courseProgressRepository.GetByUserAndCourseAsync(studentId, courseId);

                // Map dữ liệu sang DTO
                var studentDetailDto = new StudentDetailInCourseDto
                {
                    UserId = student.UserId,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    DisplayName = $"{student.FirstName} {student.LastName}",
                    Email = student.Email,
                    DateOfBirth = student.DateOfBirth,
                    IsMale = student.IsMale,
                    CourseId = courseId,
                    CourseName = course.Title,
                    JoinedAt = userCourse.JoinedAt
                };

                // Build avatar URL nếu có
                if (!string.IsNullOrWhiteSpace(student.AvatarKey))
                {
                    studentDetailDto.AvatarUrl = BuildPublicUrl.BuildURL(AvatarBucket, student.AvatarKey);
                }

                // Thêm thông tin tiến độ nếu có
                if (courseProgress != null)
                {
                    studentDetailDto.Progress = new CourseProgressDetailDto
                    {
                        CompletedLessons = courseProgress.CompletedLessons,
                        TotalLessons = courseProgress.TotalLessons,
                        ProgressPercentage = courseProgress.ProgressPercentage,
                        IsCompleted = courseProgress.IsCompleted,
                        CompletedAt = courseProgress.CompletedAt,
                        LastUpdated = courseProgress.LastUpdated,
                        ProgressDisplay = courseProgress.GetProgressDisplay()
                    };
                }
                else
                {
                    // Nếu chưa có progress record, tạo default
                    studentDetailDto.Progress = new CourseProgressDetailDto
                    {
                        CompletedLessons = 0,
                        TotalLessons = course.Lessons?.Count ?? 0,
                        ProgressPercentage = 0,
                        IsCompleted = false,
                        CompletedAt = null,
                        LastUpdated = userCourse.JoinedAt,
                        ProgressDisplay = $"0/{course.Lessons?.Count ?? 0} (0.0%)"
                    };
                }

                response.Data = studentDetailDto;
                response.StatusCode = 200;
                response.Success = true;
                response.Message = "Lấy thông tin học sinh thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
            }
            return response;
        }

        // Teacher lấy chi tiết học sinh trong course của mình
        public async Task<ServiceResponse<StudentDetailInCourseDto>> GetStudentDetailInCourseAsync(
            int courseId, 
            int studentId,
            int teacherId)
        {
            var response = new ServiceResponse<StudentDetailInCourseDto>();
            try
            {
                var (student, userCourse, progress) = await _userRepository.GetStudentDetailDataForTeacherAsync(courseId, studentId, teacherId);

                if (student == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy học sinh hoặc bạn không có quyền truy cập";
                    return response;
                }

                if (userCourse == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Học sinh chưa tham gia khóa học này";
                    return response;
                }

                // Lấy course để có title
                var course = await _courseRepository.GetCourseById(courseId);

                // Map dữ liệu sang DTO
                var studentDetailDto = new StudentDetailInCourseDto
                {
                    UserId = student.UserId,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    DisplayName = $"{student.FirstName} {student.LastName}",
                    Email = student.Email,
                    DateOfBirth = student.DateOfBirth,
                    IsMale = student.IsMale,
                    CourseId = courseId,
                    CourseName = course?.Title ?? "Unknown",
                    JoinedAt = userCourse.JoinedAt
                };

                // Build avatar URL nếu có
                if (!string.IsNullOrWhiteSpace(student.AvatarKey))
                {
                    studentDetailDto.AvatarUrl = BuildPublicUrl.BuildURL(AvatarBucket, student.AvatarKey);
                }

                // Thêm thông tin tiến độ nếu có
                if (progress != null)
                {
                    studentDetailDto.Progress = new CourseProgressDetailDto
                    {
                        CompletedLessons = progress.CompletedLessons,
                        TotalLessons = progress.TotalLessons,
                        ProgressPercentage = progress.ProgressPercentage,
                        IsCompleted = progress.IsCompleted,
                        CompletedAt = progress.CompletedAt,
                        LastUpdated = progress.LastUpdated,
                        ProgressDisplay = progress.GetProgressDisplay()
                    };
                }
                else
                {
                    // Nếu chưa có progress record, tạo default
                    var totalLessons = course?.Lessons?.Count ?? 0;
                    studentDetailDto.Progress = new CourseProgressDetailDto
                    {
                        CompletedLessons = 0,
                        TotalLessons = totalLessons,
                        ProgressPercentage = 0,
                        IsCompleted = false,
                        CompletedAt = null,
                        LastUpdated = userCourse.JoinedAt,
                        ProgressDisplay = $"0/{totalLessons} (0.0%)"
                    };
                }

                response.Data = studentDetailDto;
                response.StatusCode = 200;
                response.Success = true;
                response.Message = "Lấy thông tin học sinh thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
            }
            return response;
        }

        // Xóa học sinh khỏi course (Admin/Teacher)
        public async Task<ServiceResponse<bool>> RemoveStudentFromCourseAsync(
            int courseId, 
            int studentId, 
            int currentUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                // Validate ownership for teachers: teacher phải sở hữu course
                var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                if (currentUser != null && currentUser.Roles.Any(r => r.Name == "Teacher"))
                {
                    if (!course.TeacherId.HasValue || course.TeacherId.Value != currentUserId)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Bạn không có quyền xóa học sinh khỏi khóa học này";
                        _logger.LogWarning("Teacher {TeacherId} attempted to remove student {StudentId} from course {CourseId} owned by {OwnerId}",
                            currentUserId, studentId, courseId, course.TeacherId);
                        return response;
                    }
                }
                // Admin có full quyền, không cần validate ownership

                // Kiểm tra student có enrolled trong course không
                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId, studentId);
                if (!isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Học sinh chưa tham gia khóa học này";
                    return response;
                }

                // Xóa student khỏi course
                await _courseRepository.UnenrollUserFromCourse(courseId, studentId);

                response.Data = true;
                response.StatusCode = 200;
                response.Success = true;
                response.Message = "Xóa học sinh khỏi khóa học thành công";

               
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
                
            }
            return response;
        }

        // Thêm học sinh vào course bằng email (Admin/Teacher)
        public async Task<ServiceResponse<bool>> AddStudentToCourseByEmailAsync(
            int courseId,
            string studentEmail,
            int currentUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                // Validate ownership for teachers: teacher phải sở hữu course
                var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                if (currentUser != null && currentUser.Roles.Any(r => r.Name == "Teacher"))
                {
                    if (!course.TeacherId.HasValue || course.TeacherId.Value != currentUserId)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Bạn không có quyền thêm học sinh vào khóa học này";
                        _logger.LogWarning("Teacher {TeacherId} attempted to add student to course {CourseId} owned by {OwnerId}",
                            currentUserId, courseId, course.TeacherId);
                        return response;
                    }
                }
                // Admin có full quyền, không cần validate ownership

                // Tìm user theo email
                var student = await _userRepository.GetUserByEmailAsync(studentEmail);
                if (student == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = $"Không tìm thấy người dùng với email: {studentEmail}";
                    return response;
                }

                // Kiểm tra đã enrolled chưa
                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId, student.UserId);
                if (isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Học sinh đã tham gia khóa học này rồi";
                    return response;
                }

                // Kiểm tra course đã full chưa (nếu có MaxStudent)
                if (course.MaxStudent > 0 && course.EnrollmentCount >= course.MaxStudent)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Khóa học đã đầy ({course.EnrollmentCount}/{course.MaxStudent} học sinh)";
                    return response;
                }

                // Thêm student vào course
                await _courseRepository.EnrollUserInCourse(student.UserId, courseId);

                response.Data = true;
                response.StatusCode = 200;
                response.Success = true;
                response.Message = $"Đã thêm học sinh {student.FirstName} {student.LastName} vào khóa học thành công";

                // Log action (important for audit trail)
                // _logger.LogInformation("{Role} {UserId} added student {StudentId} ({Email}) to course {CourseId}",
                //     currentUserRole, currentUserId, student.UserId, studentEmail, courseId);
            }
            catch (InvalidOperationException ex)
            {
                // Catch specific exception từ EnrollUserInCourse (already enrolled)
                response.Success = false;
                response.StatusCode = 400;
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
                // _logger.LogError(ex, "Error adding student {Email} to course {CourseId} by {Role} {UserId}",
                //     studentEmail, courseId, currentUserRole, currentUserId);
            }
            return response;
        }
    }
}
