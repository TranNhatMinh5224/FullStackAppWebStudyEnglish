using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Application.Common;
using CleanDemo.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;


namespace CleanDemo.Application.Service
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

        public async Task<ServiceResponse<List<TeacherPackageDto>>> GetAllTeacherPackagesAsync()
        {
            var response = new ServiceResponse<List<TeacherPackageDto>>();
            try
            {
                var teacherPackages = await _teacherPackageRepository.GetAllTeacherPackagesAsync();
                response.Data = _mapper.Map<List<TeacherPackageDto>>(teacherPackages);
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all teacher packages.");
                response.Success = false;
                response.Message = "An error occurred while retrieving teacher packages.";
            }
            return response;
        }
        public async Task<ServiceResponse<TeacherPackageDto>> GetTeacherPackageByIdAsync(int id)
        {
            var response = new ServiceResponse<TeacherPackageDto>();
            try
            {
                var teacherPackage = await _teacherPackageRepository.GetTeacherPackageByIdAsync(id);
                if (teacherPackage != null)
                {
                    response.Data = _mapper.Map<TeacherPackageDto>(teacherPackage);
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Teacher package not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving teacher package with ID {id}.");
                response.Success = false;
                response.Message = "An error occurred while retrieving the teacher package.";
            }
            return response;
        }
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
                    response.Message = "Teacher package with the same name already exists.";
                    return response;
                }

                var teacherPackage = _mapper.Map<TeacherPackage>(dto);
                await _teacherPackageRepository.AddTeacherPackageAsync(teacherPackage);
                response.Data = _mapper.Map<TeacherPackageDto>(teacherPackage);
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating teacher package.");
                response.Success = false;
                response.Message = "An error occurred while creating the teacher package.";
            }
            return response;
        }
        public async Task<ServiceResponse<TeacherPackageDto>> UpdateTeacherPackageAsync(int id, UpdateTeacherPackageDto dto)
        {
            var response = new ServiceResponse<TeacherPackageDto>();
            try
            {
                // Kiểm tra xem gói giáo viên có tồn tại không
                var existingPackage = await _teacherPackageRepository.GetTeacherPackageByIdAsync(id);
                if (existingPackage == null)
                {
                    response.Success = false;
                    response.Message = "Teacher package not found.";
                    return response;
                }

                // Kiểm tra trùng lặp tên, loại trừ bản ghi hiện tại
                var allPackages = await _teacherPackageRepository.GetAllTeacherPackagesAsync();
                if (allPackages.Any(p => p.PackageName == dto.PackageName && p.TeacherPackageId != id))
                {
                    response.Success = false;
                    response.Message = "Teacher package with the same name already exists.";
                    return response;
                }

                var teacherPackage = _mapper.Map<TeacherPackage>(dto);
                teacherPackage.TeacherPackageId = id;
                await _teacherPackageRepository.UpdateTeacherPackageAsync(teacherPackage);
                response.Data = _mapper.Map<TeacherPackageDto>(teacherPackage);
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating teacher package.");
                response.Success = false;
                response.Message = "An error occurred while updating the teacher package.";
            }
            return response;
        }
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
                    response.Message = "Teacher package not found.";
                    return response;
                }

                await _teacherPackageRepository.DeleteTeacherPackageAsync(id);
                response.Data = true;
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting teacher package with ID {id}.");
                response.Data = false;
                response.Success = false;
                response.Message = "An error occurred while deleting the teacher package.";
            }
            return response;
        }

    }
}