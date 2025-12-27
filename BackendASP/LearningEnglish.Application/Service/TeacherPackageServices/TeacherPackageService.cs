using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Services.TeacherPackage;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;

using LearningEnglish.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;


namespace LearningEnglish.Application.Service
{
    public class TeacherPackageService : ITeacherPackageService
    {
        private readonly ITeacherPackageRepository _teacherPackageRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherPackageService> _logger;

        public TeacherPackageService(ITeacherPackageRepository teacherPackageRepository, IMapper mapper, ILogger<TeacherPackageService> logger)
        {
            _teacherPackageRepository = teacherPackageRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // Chỉ Admin mới có thể CRUD (đã có Permission check ở controller)
        public async Task<ServiceResponse<List<TeacherPackageDto>>> GetAllTeacherPackagesAsync()
        {
            var response = new ServiceResponse<List<TeacherPackageDto>>();
            try
            {
                var teacherPackages = await _teacherPackageRepository.GetAllTeacherPackagesAsync();
                response.StatusCode = 200;
                response.Data = _mapper.Map<List<TeacherPackageDto>>(teacherPackages);
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all teacher packages.");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi lấy danh sách gói giáo viên";
            }
            return response;
        }
        // Lấy TeacherPackage theo ID 
        public async Task<ServiceResponse<TeacherPackageDto>> GetTeacherPackageByIdAsync(int id)
        {
            var response = new ServiceResponse<TeacherPackageDto>();
            try
            {
                var teacherPackage = await _teacherPackageRepository.GetTeacherPackageByIdAsync(id);
                if (teacherPackage != null)
                {
                    response.StatusCode = 200;
                    response.Data = _mapper.Map<TeacherPackageDto>(teacherPackage);
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy gói giáo viên";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving teacher package with ID {id}.");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi lấy thông tin gói giáo viên";
            }
            return response;
        }
        // Tạo TeacherPackage mới 
        public async Task<ServiceResponse<TeacherPackageDto>> CreateTeacherPackageAsync(CreateTeacherPackageDto dto)
        {
            var response = new ServiceResponse<TeacherPackageDto>();
            try
            {
                // kiểm tra xem gói giáo viên đã tồn tại chưa
                var existingPackage = await _teacherPackageRepository.GetAllTeacherPackagesAsync();
                if (existingPackage.Any(p => p.PackageName == dto.PackageName))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Gói giáo viên với tên này đã tồn tại";
                    return response;
                }

                var teacherPackage = _mapper.Map<TeacherPackage>(dto);
                await _teacherPackageRepository.AddTeacherPackageAsync(teacherPackage);
                response.StatusCode = 201;
                response.Data = _mapper.Map<TeacherPackageDto>(teacherPackage);
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating teacher package.");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi tạo gói giáo viên";
            }
            return response;
        }
        // Cập nhật TeacherPackage theo ID
        public async Task<ServiceResponse<TeacherPackageDto>> UpdateTeacherPackageAsync(int id, UpdateTeacherPackageDto dto)
        {
            var response = new ServiceResponse<TeacherPackageDto>();
            try
            {
                var existingPackage = await _teacherPackageRepository.GetTeacherPackageByIdAsync(id);
                if (existingPackage == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy gói giáo viên";
                    return response;
                }

                // Partial update - chỉ cập nhật những trường không null
                if (!string.IsNullOrWhiteSpace(dto.PackageName))
                    existingPackage.PackageName = dto.PackageName;

                if (dto.Level.HasValue)
                    existingPackage.Level = dto.Level.Value;

                if (dto.Price.HasValue)
                    existingPackage.Price = dto.Price.Value;

                if (dto.MaxCourses.HasValue)
                    existingPackage.MaxCourses = dto.MaxCourses.Value;

                if (dto.MaxLessons.HasValue)
                    existingPackage.MaxLessons = dto.MaxLessons.Value;

                if (dto.MaxStudents.HasValue)
                    existingPackage.MaxStudents = dto.MaxStudents.Value;

                await _teacherPackageRepository.UpdateTeacherPackageAsync(existingPackage);

                var result = _mapper.Map<TeacherPackageDto>(existingPackage);
                return new ServiceResponse<TeacherPackageDto>
                {
                    StatusCode = 200,
                    Data = result,
                    Success = true,
                    Message = "Cập nhật gói giáo viên thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating teacher package.");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi cập nhật gói giáo viên";
            }
            return response;
        }
        // Xóa TeacherPackage theo ID
        public async Task<ServiceResponse<bool>> DeleteTeacherPackageAsync(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // Kiểm tra xem gói giáo viên có tồn tại không
                var existingPackage = await _teacherPackageRepository.GetTeacherPackageByIdAsync(id);
                if (existingPackage == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy gói giáo viên";
                    return response;
                }

                await _teacherPackageRepository.DeleteTeacherPackageAsync(id);
                response.StatusCode = 200;
                response.Data = true;
                response.Success = true;
                response.Message = "Xóa gói giáo viên thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting teacher package with ID {id}.");
                response.Data = false;
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi xóa gói giáo viên";
            }
            return response;
        }

    }
}
