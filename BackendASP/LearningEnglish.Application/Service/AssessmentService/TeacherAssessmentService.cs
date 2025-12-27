using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class TeacherAssessmentService : ITeacherAssessmentService
    {
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherAssessmentService> _logger;

        public TeacherAssessmentService(
            IAssessmentRepository assessmentRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<TeacherAssessmentService> logger)
        {
            _assessmentRepository = assessmentRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<AssessmentDto>> CreateAssessmentAsync(CreateAssessmentDto dto, int teacherId)
        {
            var response = new ServiceResponse<AssessmentDto>();
            try
            {
                var moduleExists = await _assessmentRepository.ModuleExists(dto.ModuleId);
                if (!moduleExists)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Module";
                    return response;
                }

                var isOwner = await _assessmentRepository.IsTeacherOwnerOfModule(teacherId, dto.ModuleId);
                if (!isOwner)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền tạo Assessment cho Module này";
                    _logger.LogWarning("Teacher {TeacherId} attempted to create assessment for module {ModuleId} without ownership", 
                        teacherId, dto.ModuleId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được tạo assessment
                var module = await _moduleRepository.GetModuleWithCourseForTeacherAsync(dto.ModuleId, teacherId);
                if (module?.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể tạo assessment cho khóa học của giáo viên";
                    _logger.LogWarning("Teacher {TeacherId} attempted to create assessment for System course module {ModuleId}", 
                        teacherId, dto.ModuleId);
                    return response;
                }

                var assessment = _mapper.Map<Assessment>(dto);
                await _assessmentRepository.AddAssessment(assessment);

                var assessmentDto = _mapper.Map<AssessmentDto>(assessment);

                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Tạo Assessment thành công";
                response.Data = assessmentDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Assessment");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi tạo Assessment";
                return response;
            }
        }

        public async Task<ServiceResponse<List<AssessmentDto>>> GetAssessmentsByModuleIdAsync(int moduleId, int teacherId)
        {
            var response = new ServiceResponse<List<AssessmentDto>>();
            try
            {
                // Lấy module với course để check
                var module = await _moduleRepository.GetModuleWithCourseAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                // Check: teacher phải là owner HOẶC đã enroll
                var courseId = module.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var course = await _courseRepository.GetCourseById(courseId.Value);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isOwner = course.TeacherId.HasValue && course.TeacherId.Value == teacherId;
                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, teacherId);

                if (!isOwner && !isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem các assessment";
                    _logger.LogWarning("Teacher {TeacherId} attempted to list assessments of module {ModuleId} without ownership or enrollment", 
                        teacherId, moduleId);
                    return response;
                }

                var assessments = await _assessmentRepository.GetAssessmentsByModuleId(moduleId);
                var assessmentDtos = _mapper.Map<List<AssessmentDto>>(assessments);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách Assessments thành công";
                response.Data = assessmentDtos;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách Assessments");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy danh sách Assessments";
                return response;
            }
        }

        public async Task<ServiceResponse<AssessmentDto>> GetAssessmentByIdAsync(int assessmentId, int teacherId)
        {
            var response = new ServiceResponse<AssessmentDto>();
            try
            {
                var assessment = await _assessmentRepository.GetAssessmentById(assessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Assessment";
                    return response;
                }

                // Lấy module với course để check
                var module = await _moduleRepository.GetModuleWithCourseAsync(assessment.ModuleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                // Check: teacher phải là owner HOẶC đã enroll
                var courseId = module.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var course = await _courseRepository.GetCourseById(courseId.Value);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isOwner = course.TeacherId.HasValue && course.TeacherId.Value == teacherId;
                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, teacherId);

                if (!isOwner && !isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem assessment này";
                    _logger.LogWarning("Teacher {TeacherId} attempted to access assessment {AssessmentId} without ownership or enrollment", 
                        teacherId, assessmentId);
                    return response;
                }

                var assessmentDto = _mapper.Map<AssessmentDto>(assessment);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy Assessment thành công";
                response.Data = assessmentDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy Assessment");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy Assessment";
                return response;
            }
        }

        public async Task<ServiceResponse<AssessmentDto>> UpdateAssessmentAsync(int assessmentId, UpdateAssessmentDto dto, int teacherId)
        {
            var response = new ServiceResponse<AssessmentDto>();
            try
            {
                var assessment = await _assessmentRepository.GetAssessmentById(assessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Assessment";
                    return response;
                }

                var isOwner = await _assessmentRepository.IsTeacherOwnerOfModule(teacherId, assessment.ModuleId);
                if (!isOwner)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền cập nhật Assessment này";
                    _logger.LogWarning("Teacher {TeacherId} attempted to update assessment {AssessmentId} without ownership", 
                        teacherId, assessmentId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được cập nhật assessment
                var module = await _moduleRepository.GetModuleWithCourseForTeacherAsync(assessment.ModuleId, teacherId);
                if (module?.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể cập nhật assessment của khóa học giáo viên";
                    _logger.LogWarning("Teacher {TeacherId} attempted to update assessment {AssessmentId} of System course", 
                        teacherId, assessmentId);
                    return response;
                }

                _mapper.Map(dto, assessment);
                await _assessmentRepository.UpdateAssessment(assessment);

                var assessmentDto = _mapper.Map<AssessmentDto>(assessment);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Cập nhật Assessment thành công";
                response.Data = assessmentDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật Assessment");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi cập nhật Assessment";
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> DeleteAssessmentAsync(int assessmentId, int teacherId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var assessment = await _assessmentRepository.GetAssessmentById(assessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Assessment";
                    response.Data = false;
                    return response;
                }

                var isOwner = await _assessmentRepository.IsTeacherOwnerOfModule(teacherId, assessment.ModuleId);
                if (!isOwner)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền xóa Assessment này";
                    response.Data = false;
                    _logger.LogWarning("Teacher {TeacherId} attempted to delete assessment {AssessmentId} without ownership", 
                        teacherId, assessmentId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được xóa assessment
                var module = await _moduleRepository.GetModuleWithCourseForTeacherAsync(assessment.ModuleId, teacherId);
                if (module?.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể xóa assessment của khóa học giáo viên";
                    response.Data = false;
                    _logger.LogWarning("Teacher {TeacherId} attempted to delete assessment {AssessmentId} of System course", 
                        teacherId, assessmentId);
                    return response;
                }

                await _assessmentRepository.DeleteAssessment(assessmentId);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa Assessment thành công";
                response.Data = true;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa Assessment");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi xóa Assessment";
                response.Data = false;
                return response;
            }
        }
    }
}
