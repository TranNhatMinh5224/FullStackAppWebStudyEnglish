using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using AutoMapper;


namespace LearningEnglish.Application.Service
{
    public class RegisterService : IRegisterService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

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

        public async Task<ServiceResponse<UserDto>> RegisterUserAsync(RegisterUserDto dto)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                // Check email đã tồn tại
                var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Email đã tồn tại trong hệ thống";
                    return response;
                }

                // Check số điện thoại đã tồn tại
                var existingPhone = await _userRepository.GetUserByPhoneNumberAsync(dto.PhoneNumber);
                if (existingPhone != null)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Số điện thoại đã tồn tại trong hệ thống";
                    return response;
                }

                var user = _mapper.Map<User>(dto);
                user.SetPassword(dto.Password);
                user.NormalizedEmail = dto.Email.ToUpper();

                // XÓA tất cả OTP cũ của email này trước khi tạo mới
                // Đảm bảo chỉ có 1 OTP mới nhất được sử dụng
                var oldTokens = await _emailVerificationTokenRepository.GetAllByEmailAsync(dto.Email);
                foreach (var oldToken in oldTokens)
                {
                    await _emailVerificationTokenRepository.DeleteAsync(oldToken);
                }
                await _emailVerificationTokenRepository.SaveChangesAsync();

                // Generate 6-digit OTP code
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();
                var emailToken = new EmailVerificationToken
                {
                    User = user,
                    OtpCode = otpCode,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15) // token hết hạn sau 15 phút
                };
                await _emailVerificationTokenRepository.AddAsync(emailToken);
                await _emailVerificationTokenRepository.SaveChangesAsync();

                // Send OTP email via EmailSender
                await _emailSender.SendEmailAsync(dto.Email, "Xác thực tài khoản", $"Mã OTP của bạn là: {otpCode}");

                response.StatusCode = 200;
                response.Data = _mapper.Map<UserDto>(user);
                response.Message = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.";
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
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

                // Check token đã hết hạn chưa
                if (token.ExpiresAt < DateTime.UtcNow)
                {
                    // XÓA OTP hết hạn - không còn khả năng sử dụng
                    await _emailVerificationTokenRepository.DeleteAsync(token);

                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP đã hết hạn";
                    response.Data = false;
                    return response;
                }

                // Check if already used
                if (token.IsUsed)
                {
                    // XÓA OTP đã sử dụng - không còn khả năng sử dụng
                    await _emailVerificationTokenRepository.DeleteAsync(token);

                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP đã được sử dụng";
                    response.Data = false;
                    return response;
                }

                // ANTI-SPAM CHECK: Kiểm tra xem có đang bị block không
                if (token.BlockedUntil.HasValue && token.BlockedUntil.Value > DateTime.UtcNow)
                {
                    var remainingMinutes = Math.Ceiling((token.BlockedUntil.Value - DateTime.UtcNow).TotalMinutes);
                    response.Success = false;
                    response.StatusCode = 429;
                    response.Message = $"Tài khoản tạm khóa đến {token.BlockedUntil.Value.AddHours(7):HH:mm dd/MM/yyyy}. Vui lòng thử lại sau {remainingMinutes} phút";
                    response.Data = false;
                    return response;
                }

                // Verify OTP code
                if (token.OtpCode != dto.OtpCode)
                {
                    // BRUTE-FORCE PROTECTION: Tăng số lần thử sai
                    token.AttemptsCount++;

                    // Nếu nhập sai >= 5 lần, khóa 20 phút
                    if (token.AttemptsCount >= 5)
                    {
                        token.BlockedUntil = DateTime.UtcNow.AddMinutes(20);
                        await _emailVerificationTokenRepository.UpdateAsync(token);

                        // XÓA OTP bị khóa - không còn khả năng sử dụng
                        await _emailVerificationTokenRepository.DeleteAsync(token);

                        response.Success = false;
                        response.StatusCode = 429;
                        response.Message = "Bạn đã nhập sai OTP quá 5 lần. Tài khoản bị khóa trong 20 phút";
                        response.Data = false;
                        return response;
                    }

                    // Nếu nhập sai >= 10 lần, mark as used
                    if (token.AttemptsCount >= 10)
                    {
                        // XÓA OTP quá nhiều lần thử - không còn khả năng sử dụng
                        await _emailVerificationTokenRepository.DeleteAsync(token);

                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Quá nhiều lần thử. Vui lòng yêu cầu mã OTP mới";
                        response.Data = false;
                        return response;
                    }

                    await _emailVerificationTokenRepository.UpdateAsync(token);

                    var remainingAttempts = 5 - token.AttemptsCount;
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Mã OTP không chính xác. Còn {remainingAttempts} lần thử";
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
