using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Auth;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using AutoMapper;
using Microsoft.Extensions.Logging;


namespace LearningEnglish.Application.Service
{
    public class InformationUserService : IInformationUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMinioFileStorage _minioFileStorage;
        private readonly ILogger<InformationUserService> _logger;
        private readonly IStreakService _streakService;
        private readonly ITeacherSubscriptionRepository _teacherSubscriptionRepository;

        // Bucket + folder cho avatar người dùng
        private const string AvatarBucket = "avatars";
        private const string AvatarFolder = "real";

        public InformationUserService(
            IUserRepository userRepository,
            IMapper mapper,
            IMinioFileStorage minioFileStorage,
            ILogger<InformationUserService> logger,
            IStreakService streakService,
            ITeacherSubscriptionRepository teacherSubscriptionRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _minioFileStorage = minioFileStorage;
            _logger = logger;
            _streakService = streakService;
            _teacherSubscriptionRepository = teacherSubscriptionRepository;
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
                _logger.LogInformation("Retrieved user profile for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error retrieving user profile for user {UserId}", userId);
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
                _logger.LogInformation("User {UserId} updated their profile", userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error updating user profile for user {UserId}", userId);
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
                _logger.LogInformation("User {UserId} updated their avatar", userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                response.Data = false;
                _logger.LogError(ex, "Error updating avatar for user {UserId}", userId);
            }
            return response;
        }
    }
}
