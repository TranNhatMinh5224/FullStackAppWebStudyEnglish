using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
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

                // Lưu user vào DB (trạng thái pending)
                await _userRepository.AddUserAsync(user);
                await _userRepository.SaveChangesAsync();

                // Generate 6-digit OTP code
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();
                var emailToken = new EmailVerificationToken
                {
                    User = user,
                    OtpCode = otpCode,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5), // token hết hạn sau 5 phút
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

                //  CHECK: Kiểm tra xem có đang bị block không
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

                    // Nếu nhập sai >= 5 lần, xóa token
                    if (token.AttemptsCount >= 5)
                    {
                        // XÓA OTP sau 5 lần thử sai - không còn khả năng sử dụng
                        await _emailVerificationTokenRepository.DeleteAsync(token);

                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Bạn đã nhập sai OTP quá 5 lần. Vui lòng đăng ký lại";
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
