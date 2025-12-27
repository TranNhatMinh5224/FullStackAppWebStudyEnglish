using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Lecture;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.LectureService
{
    public class TeacherLectureQueryService : ITeacherLectureQueryService
    {
        private readonly ILectureRepository _lectureRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherLectureQueryService> _logger;

        // Đặt bucket cho media lecture
        private const string LectureMediaBucket = "lectures";

        public TeacherLectureQueryService(
            ILectureRepository lectureRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<TeacherLectureQueryService> logger)
        {
            _lectureRepository = lectureRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // Teacher lấy lecture theo ID - Teacher có thể xem nếu là owner HOẶC đã enroll
        public async Task<ServiceResponse<LectureDto>> GetLectureByIdAsync(int lectureId, int teacherId)
        {
            var response = new ServiceResponse<LectureDto>();

            try
            {
                // Lấy lecture với module và course để check
                var lecture = await _lectureRepository.GetLectureWithModuleCourseAsync(lectureId);
                if (lecture == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy lecture";
                    return response;
                }

                // Check: teacher phải là owner HOẶC đã enroll
                var courseId = lecture.Module?.Lesson?.CourseId;
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
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem lecture này";
                    _logger.LogWarning("Teacher {TeacherId} attempted to access lecture {LectureId} without ownership or enrollment", 
                        teacherId, lectureId);
                    return response;
                }

                // Load full details
                var lectureWithDetails = await _lectureRepository.GetByIdWithDetailsAsync(lectureId);
                if (lectureWithDetails == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy lecture";
                    return response;
                }

                var lectureDto = _mapper.Map<LectureDto>(lectureWithDetails);

                // Generate URL từ key cho MediaUrl
                if (!string.IsNullOrWhiteSpace(lectureDto.MediaUrl))
                {
                    lectureDto.MediaUrl = BuildPublicUrl.BuildURL(
                        LectureMediaBucket,
                        lectureDto.MediaUrl
                    );
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Data = lectureDto;
                response.Message = "Lấy thông tin lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lecture với ID: {LectureId}", lectureId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy thông tin lecture";
            }

            return response;
        }

        // Teacher lấy danh sách lecture theo module - Teacher có thể xem nếu là owner HOẶC đã enroll
        public async Task<ServiceResponse<List<ListLectureDto>>> GetLecturesByModuleIdAsync(int moduleId, int teacherId)
        {
            var response = new ServiceResponse<List<ListLectureDto>>();

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
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem các lecture";
                    _logger.LogWarning("Teacher {TeacherId} attempted to list lectures of module {ModuleId} without ownership or enrollment", 
                        teacherId, moduleId);
                    return response;
                }

                var lectures = await _lectureRepository.GetByModuleIdWithDetailsAsync(moduleId);
                var lectureDtos = _mapper.Map<List<ListLectureDto>>(lectures);

                // Generate URLs cho tất cả lectures
                foreach (var dto in lectureDtos)
                {
                    if (!string.IsNullOrWhiteSpace(dto.MediaUrl))
                    {
                        dto.MediaUrl = BuildPublicUrl.BuildURL(LectureMediaBucket, dto.MediaUrl);
                    }
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Data = lectureDtos;
                response.Message = $"Lấy danh sách {lectures.Count} lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lecture theo ModuleId: {ModuleId}", moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy danh sách lecture";
            }

            return response;
        }

        // Teacher lấy cây lecture - Teacher có thể xem nếu là owner HOẶC đã enroll
        public async Task<ServiceResponse<List<LectureTreeDto>>> GetLectureTreeByModuleIdAsync(int moduleId, int teacherId)
        {
            var response = new ServiceResponse<List<LectureTreeDto>>();

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
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem cây lecture";
                    _logger.LogWarning("Teacher {TeacherId} attempted to get lecture tree of module {ModuleId} without ownership or enrollment", 
                        teacherId, moduleId);
                    return response;
                }

                var lectures = await _lectureRepository.GetTreeByModuleIdAsync(moduleId);

                // Tạo cấu trúc cây
                var rootLectures = lectures.Where(l => l.ParentLectureId == null).ToList();
                var treeDtos = new List<LectureTreeDto>();

                foreach (var rootLecture in rootLectures)
                {
                    var treeDto = _mapper.Map<LectureTreeDto>(rootLecture);
                    BuildLectureTree(treeDto, lectures);
                    treeDtos.Add(treeDto);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Data = treeDtos;
                response.Message = $"Lấy cây lecture thành công với {treeDtos.Count} root lectures";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cây lecture theo ModuleId: {ModuleId}", moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy cây lecture";
            }

            return response;
        }

        private void BuildLectureTree(LectureTreeDto parent, List<Domain.Entities.Lecture> allLectures)
        {
            var children = allLectures.Where(l => l.ParentLectureId == parent.LectureId).ToList();

            foreach (var child in children.OrderBy(c => c.OrderIndex))
            {
                var childDto = _mapper.Map<LectureTreeDto>(child);
                parent.Children.Add(childDto);
                BuildLectureTree(childDto, allLectures);
            }
        }
    }
}