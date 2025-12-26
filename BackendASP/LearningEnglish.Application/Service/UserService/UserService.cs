using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;


namespace LearningEnglish.Application.Service
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly IMinioFileStorage _minioFileStorage;
        private readonly IStreakService _streakService;
        private readonly ITeacherSubscriptionRepository _teacherSubscriptionRepository;
        private readonly ICourseProgressRepository _courseProgressRepository;
        private readonly ILogger<UserManagementService> _logger;

        // Bucket + folder cho avatar người dùng
        private const string AvatarBucket = "avatars";
        private const string AvatarFolder = "real";

        public UserManagementService(
            IUserRepository userRepository,
            IMapper mapper,
            ICourseRepository courseRepository,
            IMinioFileStorage minioFileStorage,
            IStreakService streakService,
            ITeacherSubscriptionRepository teacherSubscriptionRepository,
            ICourseProgressRepository courseProgressRepository,
            ILogger<UserManagementService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _courseRepository = courseRepository;
            _minioFileStorage = minioFileStorage;
            _streakService = streakService;
            _teacherSubscriptionRepository = teacherSubscriptionRepository;
            _courseProgressRepository = courseProgressRepository;
            _logger = logger;
        }




        // lấy ra thông tin hồ sơ người dùng

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

                // Build URL cho avatar nếu tồn tại
                if (!string.IsNullOrWhiteSpace(user.AvatarKey))
                {
                    response.Data.AvatarUrl = BuildPublicUrl.BuildURL(AvatarBucket, user.AvatarKey);
                }

                // Get streak info
                var streakResult = await _streakService.GetCurrentStreakAsync(userId);
                if (streakResult.Success && streakResult.Data != null)
                {
                    response.Data.Streak = streakResult.Data;
                }

                // Lấy ra active teacher subscription nếu có
                var subscription = await _teacherSubscriptionRepository.GetActiveSubscriptionAsync(userId);
                response.Data.TeacherSubscription = subscription != null
                    ? _mapper.Map<UserTeacherSubscriptionDto>(subscription)
                    : new UserTeacherSubscriptionDto { IsTeacher = false, PackageLevel = null };
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }


        // cập nhật hồ sơ người dùng

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

                // Check số điện thoại trùng (nếu thay đổi)
                if (dto.PhoneNumber != user.PhoneNumber)
                {
                    var existingPhone = await _userRepository.GetUserByPhoneNumberAsync(dto.PhoneNumber);
                    if (existingPhone != null && existingPhone.UserId != userId)
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Số điện thoại đã tồn tại trong hệ thống";
                        return response;
                    }
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



        // update avatar người dùng

        public async Task<ServiceResponse<bool>> UpdateAvatarAsync(int userId, UpdateAvatarDto dto)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy người dùng";
                    response.Data = false;
                    return response;
                }

                string? committedAvatarKey = null;

                // Convert temp file → real file nếu có AvatarTempKey
                if (!string.IsNullOrWhiteSpace(dto.AvatarTempKey))
                {
                    var commitResult = await _minioFileStorage.CommitFileAsync(
                        dto.AvatarTempKey,
                        AvatarBucket,
                        AvatarFolder
                    );

                    if (!commitResult.Success || string.IsNullOrWhiteSpace(commitResult.Data))
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu avatar. Vui lòng thử lại.";
                        response.Data = false;
                        return response;
                    }

                    committedAvatarKey = commitResult.Data;

                    // Xóa avatar cũ nếu tồn tại
                    if (!string.IsNullOrWhiteSpace(user.AvatarKey))
                    {
                        await _minioFileStorage.DeleteFileAsync(user.AvatarKey, AvatarBucket);
                    }

                    user.AvatarKey = committedAvatarKey;
                }

                try
                {
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateUserAsync(user);
                    await _userRepository.SaveChangesAsync();
                }
                catch (Exception)
                {
                    // Rollback: xóa file đã commit nếu DB thất bại
                    if (!string.IsNullOrWhiteSpace(committedAvatarKey))
                    {
                        await _minioFileStorage.DeleteFileAsync(committedAvatarKey, AvatarBucket);
                    }
                    throw;
                }

                response.StatusCode = 200;
                response.Success = true;
                response.Message = "Cập nhật avatar thành công";
                response.Data = true;
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                response.Data = false;
            }
            return response;
        }
        // Lấy tất cả người dùng với phân trang, search và sort
        public async Task<ServiceResponse<PagedResult<UserDto>>> GetAllUsersPagedAsync(UserQueryParameters request)
        {
            var response = new ServiceResponse<PagedResult<UserDto>>();
            try
            {
                // Directly pass UserQueryParameters to repository
                var pagedData = await _userRepository.GetAllUsersPagedAsync(request);

                var result = new PagedResult<UserDto>
                {
                    Items = _mapper.Map<List<UserDto>>(pagedData.Items),
                    TotalCount = pagedData.TotalCount,
                    PageNumber = pagedData.PageNumber,
                    PageSize = pagedData.PageSize
                };

                response.StatusCode = 200;
                response.Data = result;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi: {ex.Message}";
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

        // Danh sách tài khoản bị khóa với phân trang
        public async Task<ServiceResponse<PagedResult<UserDto>>> GetListBlockedAccountsPagedAsync(UserQueryParameters request)
        {
            var response = new ServiceResponse<PagedResult<UserDto>>();
            try
            {
                // Directly pass UserQueryParameters to repository
                var pagedData = await _userRepository.GetListBlockedAccountsPagedAsync(request);

                var result = new PagedResult<UserDto>
                {
                    Items = _mapper.Map<List<UserDto>>(pagedData.Items),
                    TotalCount = pagedData.TotalCount,
                    PageNumber = pagedData.PageNumber,
                    PageSize = pagedData.PageSize
                };

                response.StatusCode = 200;
                response.Success = true;
                response.Data = result;
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // Lấy danh sách người dùng theo khóa học với phân trang
        // RLS tự động filter: Admin xem tất cả, Teacher chỉ xem students trong own courses
        // userId không cần (RLS đã filter), chỉ cần để log ở Controller
        public async Task<ServiceResponse<PagedResult<UserDto>>> GetUsersByCourseIdPagedAsync(int courseId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<UserDto>>();
            try
            {
                // RLS đã tự động filter courses theo role (Admin: all, Teacher: own)
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học hoặc bạn không có quyền truy cập";
                    return response;
                }

                // RLS policy đã tự động filter UserCourses
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

        // Lấy danh sách giáo viên theo phân trang
        public async Task<ServiceResponse<PagedResult<UserDto>>> GetListTeachersPagedAsync(UserQueryParameters request)
        {
            var response = new ServiceResponse<PagedResult<UserDto>>();
            try
            {
                // Directly pass UserQueryParameters to repository
                var pagedData = await _userRepository.GetAllTeachersPagedAsync(request);

                var result = new PagedResult<UserDto>
                {
                    Items = _mapper.Map<List<UserDto>>(pagedData.Items),
                    TotalCount = pagedData.TotalCount,
                    PageNumber = pagedData.PageNumber,
                    PageSize = pagedData.PageSize
                };

                response.StatusCode = 200;
                response.Success = true;
                response.Data = result;
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        // Lấy thông tin chi tiết của học sinh trong một course cụ thể
        // RLS tự động filter: Admin xem tất cả, Teacher chỉ xem students trong own courses
        // currentUserId không cần (RLS đã filter), chỉ cần để log ở Controller
        public async Task<ServiceResponse<StudentDetailInCourseDto>> GetStudentDetailInCourseAsync(
            int courseId, 
            int studentId)
        {
            var response = new ServiceResponse<StudentDetailInCourseDto>();
            try
            {
                // RLS đã tự động filter courses theo role (Admin: all, Teacher: own)
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

        // Xóa học sinh khỏi course (Admin/Teacher)
        // RLS tự động filter: Admin xóa bất kỳ student nào, Teacher chỉ xóa students trong own courses
        public async Task<ServiceResponse<bool>> RemoveStudentFromCourseAsync(
            int courseId, 
            int studentId, 
            int currentUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // RLS đã tự động filter courses theo role (Admin: all, Teacher: own)
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học hoặc bạn không có quyền truy cập";
                    return response;
                }

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

                // Log action (important for audit trail)
                // _logger.LogInformation("{Role} {UserId} removed student {StudentId} from course {CourseId}", 
                //     currentUserRole, currentUserId, studentId, courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
                // _logger.LogError(ex, "Error removing student {StudentId} from course {CourseId} by {Role} {UserId}", 
                //     studentId, courseId, currentUserRole, currentUserId);
            }
            return response;
        }

        // Thêm học sinh vào course bằng email (Admin/Teacher)
        // RLS tự động filter: Admin thêm vào bất kỳ course nào, Teacher chỉ thêm vào own courses
        public async Task<ServiceResponse<bool>> AddStudentToCourseByEmailAsync(
            int courseId,
            string studentEmail,
            int currentUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // RLS đã tự động filter courses theo role (Admin: all, Teacher: own)
                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học hoặc bạn không có quyền truy cập";
                    return response;
                }

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
