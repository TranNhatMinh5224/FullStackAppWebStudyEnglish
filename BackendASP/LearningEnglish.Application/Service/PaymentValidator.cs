using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class PaymentValidator : IPaymentValidator
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ITeacherPackageRepository _teacherPackageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<PaymentValidator> _logger;

        public PaymentValidator(
            ICourseRepository courseRepository,
            ITeacherPackageRepository teacherPackageRepository,
            IUserRepository userRepository,
            IPaymentRepository paymentRepository,
            ILogger<PaymentValidator> logger)
        {
            _courseRepository = courseRepository;
            _teacherPackageRepository = teacherPackageRepository;
            _userRepository = userRepository;
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<ServiceResponse<decimal>> ValidateProductAsync(int productId, ProductType productType)
        {
            var response = new ServiceResponse<decimal>();

            try
            {
                switch (productType)
                {
                    case ProductType.Course:
                        return await ValidateCourseAsync(productId);

                    case ProductType.TeacherPackage:
                        return await ValidateTeacherPackageAsync(productId);

                    default:
                        response.Success = false;
                        response.Message = "Loại sản phẩm không được hỗ trợ";
                        return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate sản phẩm {ProductId}, Loại {ProductType}", productId, productType);
                response.Success = false;
                response.Message = "Đã xảy ra lỗi khi validate sản phẩm";
            }

            return response;
        }

        private async Task<ServiceResponse<decimal>> ValidateCourseAsync(int courseId)
        {
            var response = new ServiceResponse<decimal>();

            var course = await _courseRepository.GetCourseById(courseId);
            if (course == null)
            {
                _logger.LogWarning("Không tìm thấy khóa học {CourseId}", courseId);
                response.Success = false;
                response.Message = "Không tìm thấy khóa học";
                return response;
            }

            if (course.Price == null || course.Price <= 0)
            {
                _logger.LogWarning("Khóa học {CourseId} có giá không hợp lệ {Price}", courseId, course.Price);
                response.Success = false;
                response.Message = "Giá khóa học không hợp lệ";
                return response;
            }

            if (!course.CanJoin())
            {
                _logger.LogWarning("Khóa học {CourseId} đã đầy (Tối đa: {MaxStudent}, Hiện tại: {EnrollmentCount})",
                    courseId, course.MaxStudent, course.EnrollmentCount);
                response.Success = false;
                response.Message = "Khóa học đã đầy";
                return response;
            }

            response.Data = course.Price.Value;
            _logger.LogInformation("Xác thực khóa học thành công: {CourseId}, Tiêu đề: {Title}, Giá: {Price}",
                course.CourseId, course.Title, course.Price);
            
            return response;
        }

        private async Task<ServiceResponse<decimal>> ValidateTeacherPackageAsync(int packageId)
        {
            var response = new ServiceResponse<decimal>();

            var package = await _teacherPackageRepository.GetTeacherPackageByIdAsync(packageId);
            if (package == null)
            {
                _logger.LogWarning("Không tìm thấy gói giáo viên {PackageId}", packageId);
                response.Success = false;
                response.Message = "Không tìm thấy gói giáo viên";
                return response;
            }

            if (package.Price <= 0)
            {
                _logger.LogWarning("Gói giáo viên {PackageId} có giá không hợp lệ {Price}", packageId, package.Price);
                response.Success = false;
                response.Message = "Giá gói giáo viên không hợp lệ";
                return response;
            }

            response.Data = package.Price;
            _logger.LogInformation("Xác thực gói giáo viên thành công: {PackageId}, Giá: {Price}",
                package.TeacherPackageId, package.Price);
            
            return response;
        }

        public async Task<ServiceResponse<bool>> ValidateUserPaymentAsync(int userId, int productId, ProductType productType)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Validate user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Không tìm thấy User {UserId}", userId);
                    response.Success = false;
                    response.Message = "Không tìm thấy người dùng";
                    return response;
                }

                // Check if user already purchased this product
                var existingPayment = await _paymentRepository.GetSuccessfulPaymentByUserAndProductAsync(userId, productId, productType);
                if (existingPayment != null)
                {
                    _logger.LogWarning("User {UserId} đã mua {ProductType} {ProductId}", userId, productType, productId);
                    response.Success = false;
                    response.Message = "Bạn đã mua sản phẩm này rồi";
                    return response;
                }

                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate user payment cho User {UserId}", userId);
                response.Success = false;
                response.Message = "Đã xảy ra lỗi khi kiểm tra thông tin thanh toán";
            }

            return response;
        }
    }
}
