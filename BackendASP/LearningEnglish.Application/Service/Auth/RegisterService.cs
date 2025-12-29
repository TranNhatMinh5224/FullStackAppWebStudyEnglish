using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface.Auth;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Common.Helpers;
using AutoMapper;


namespace LearningEnglish.Application.Service
{
    // Service xử lý đăng ký tài khoản người dùng mới
    public class RegisterService : IRegisterService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        // Constructor khởi tạo các dependency injection
        public RegisterService(
            IUserRepository userRepository,
            IEmailVerificationTokenRepository emailVerificationTokenRepository,
            IEmailSender emailSender,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _emailVerificationTokenRepository = emailVerificationTokenRepository;
            _emailSender = emailSender;
            _mapper = mapper;
        }

        // Xử lý đăng ký tài khoản người dùng mới với xác thực email
        public async Task<ServiceResponse<UserDto>> RegisterUserAsync(RegisterUserDto dto)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                // Kiểm tra email đã tồn tại và được xác thực chưa
                var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (existingUser != null && existingUser.EmailVerified)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Email đã tồn tại trong hệ thống";
                    return response;
                }

                // Nếu email tồn tại nhưng chưa xác thực - cho phép đăng ký lại
                if (existingUser != null && !existingUser.EmailVerified)
                {
                    // Xóa user cũ chưa verify và OTP cũ
                    await _userRepository.DeleteUserAsync(existingUser.UserId);
                    var oldTokens = await _emailVerificationTokenRepository.GetAllByEmailAsync(dto.Email);
                    foreach (var oldToken in oldTokens)
                    {
                        await _emailVerificationTokenRepository.DeleteAsync(oldToken);
                    }
                    await _emailVerificationTokenRepository.SaveChangesAsync();
                }

                // Check số điện thoại đã tồn tại VÀ đã verify
                var existingPhone = await _userRepository.GetUserByPhoneNumberAsync(dto.PhoneNumber);
                if (existingPhone != null && existingPhone.EmailVerified)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Số điện thoại đã tồn tại trong hệ thống";
                    return response;
                }

                // Tạo user MỚI với EmailVerified = FALSE (trạng thái chờ xác thực)
                var user = _mapper.Map<User>(dto);
                user.SetPassword(dto.Password);
                user.NormalizedEmail = dto.Email.ToUpper();
                user.EmailVerified = false;  // ← CHƯA XÁC THỰC

                // Gắn role Student mặc định cho user mới
                var studentRole = await _userRepository.GetRoleByNameAsync("Student");
                if (studentRole != null)
                {
                    user.Roles.Add(studentRole);
                }

                // Lưu user vào DB (trạng thái pending)
                await _userRepository.AddUserAsync(user);
                await _userRepository.SaveChangesAsync();

                // Generate 6-digit OTP code using OtpHelper
                var otpCode = OtpHelper.GenerateOtpCode();
                var emailToken = new EmailVerificationToken
                {
                    User = user,
                    OtpCode = otpCode,
                    ExpiresAt = OtpHelper.GetExpirationTime(5), // 5 minutes
                    Email = dto.Email  // Lưu email để query
                };
                await _emailVerificationTokenRepository.AddAsync(emailToken);
                await _emailVerificationTokenRepository.SaveChangesAsync();

                // Send OTP email via EmailSender
                await _emailSender.SendEmailAsync(dto.Email, "Xác thực tài khoản", $"Mã OTP của bạn là: {otpCode}. Mã này có hiệu lực trong 5 phút.");

                response.StatusCode = 200;
                response.Data = _mapper.Map<UserDto>(user);
                response.Message = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản trong vòng 5 phút.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Đã xảy ra lỗi hệ thống: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> VerifyEmailAsync(VerifyEmailDto dto)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var token = await _emailVerificationTokenRepository.GetByEmailAsync(dto.Email);

                // Check token tồn tại
                if (token == null)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP không hợp lệ";
                    response.Data = false;
                    return response;
                }

                // Check token đã hết hạn chưa using OtpHelper
                if (OtpHelper.IsExpired(token.ExpiresAt))
                {
                    // XÓA OTP hết hạn
                    await _emailVerificationTokenRepository.DeleteAsync(token);

                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP đã hết hạn";
                    response.Data = false;
                    return response;
                }

                // Validate OTP with brute-force protection using OtpHelper
                var validationResult = OtpHelper.ValidateOtp(dto.OtpCode, token.OtpCode, token.AttemptsCount, maxAttempts: 5);

                if (!validationResult.IsValid)
                {
                    // Handle failed validation
                    if (validationResult.Action == OtpAction.DeleteToken)
                    {
                        // Max attempts reached - delete token
                        await _emailVerificationTokenRepository.DeleteAsync(token);
                    }
                    else if (validationResult.Action == OtpAction.UpdateAttempts)
                    {
                        // Update attempts count
                        token.AttemptsCount = validationResult.NewAttemptsCount;
                        await _emailVerificationTokenRepository.UpdateAsync(token);
                        await _emailVerificationTokenRepository.SaveChangesAsync();
                    }

                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = validationResult.Message;
                    response.Data = false;
                    return response;
                }

                // OTP đúng: Cập nhật EmailVerified cho user
                var user = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (user != null)
                {
                    user.EmailVerified = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateUserAsync(user);
                    await _userRepository.SaveChangesAsync();
                }

                // Xóa OTP khỏi database sau khi xác thực thành công
                await _emailVerificationTokenRepository.DeleteAsync(token);

                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Xác thực email thành công";
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
    }
}
