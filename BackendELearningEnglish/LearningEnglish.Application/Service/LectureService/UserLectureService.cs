using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Constants;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Lecture;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    
    public class UserLectureService : IUserLectureService
    {
        private readonly ILectureRepository _lectureRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserLectureService> _logger;
        private readonly ILectureMediaService _lectureMediaService;

        public UserLectureService(
            ILectureRepository lectureRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<UserLectureService> logger,
            ILectureMediaService lectureMediaService)
        {
            _lectureRepository = lectureRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
            _lectureMediaService = lectureMediaService;
        }

        // Lấy thông tin lecture với progress của user (chỉ xem được nếu đã đăng ký course)
        public async Task<ServiceResponse<LectureDto>> GetLectureByIdAsync(int lectureId, int userId)
        {
            var response = new ServiceResponse<LectureDto>();

            try
            {
                var lecture = await _lectureRepository.GetLectureWithModuleCourseAsync(lectureId);
                if (lecture == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy lecture";
                    return response;
                }

                // Check enrollment: user phải đăng ký course mới được xem lecture
                var courseId = lecture.Module?.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, userId);
                if (!isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần đăng ký khóa học để xem lecture này";
                    _logger.LogWarning("User {UserId} attempted to access lecture {LectureId} without enrollment in course {CourseId}", 
                        userId, lectureId, courseId.Value);
                    return response;
                }

                var lectureDto = _mapper.Map<LectureDto>(lecture);

                // Generate URL từ key cho MediaUrl
                if (!string.IsNullOrWhiteSpace(lectureDto.MediaUrl))
                {
                    lectureDto.MediaUrl = _lectureMediaService.BuildMediaUrl(lectureDto.MediaUrl);
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

        // Lấy danh sách lecture theo module với progress của user (chỉ xem được nếu đã đăng ký course)
        public async Task<ServiceResponse<List<ListLectureDto>>> GetLecturesByModuleIdAsync(int moduleId, int userId)
        {
            var response = new ServiceResponse<List<ListLectureDto>>();

            try
            {
                // Lấy module để check course
                var module = await _moduleRepository.GetModuleWithCourseAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                // Check enrollment: user phải đăng ký course mới được xem lecture
                var courseId = module.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, userId);
                if (!isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần đăng ký khóa học để xem lecture";
                    _logger.LogWarning("User {UserId} attempted to list lectures of module {ModuleId} without enrollment in course {CourseId}", 
                        userId, moduleId, courseId.Value);
                    return response;
                }

                var lectures = await _lectureRepository.GetByModuleIdWithDetailsAsync(moduleId);
                var lectureDtos = _mapper.Map<List<ListLectureDto>>(lectures);

                // Generate URLs cho tất cả lectures
                foreach (var dto in lectureDtos)
                {
                    if (!string.IsNullOrWhiteSpace(dto.MediaUrl))
                    {
                        dto.MediaUrl = _lectureMediaService.BuildMediaUrl(dto.MediaUrl);
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

        // Lấy cấu trúc cây lecture theo module với progress của user (chỉ xem được nếu đã đăng ký course)
        public async Task<ServiceResponse<List<LectureTreeDto>>> GetLectureTreeByModuleIdAsync(int moduleId, int userId)
        {
            var response = new ServiceResponse<List<LectureTreeDto>>();

            try
            {
                // Lấy module để check course
                var module = await _moduleRepository.GetModuleWithCourseAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                // Check enrollment: user phải đăng ký course mới được xem lecture
                var courseId = module.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, userId);
                if (!isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần đăng ký khóa học để xem lecture";
                    _logger.LogWarning("User {UserId} attempted to get lecture tree of module {ModuleId} without enrollment in course {CourseId}", 
                        userId, moduleId, courseId.Value);
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
                response.Message = "Lấy cấu trúc cây lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cấu trúc cây lecture theo ModuleId: {ModuleId}", moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy cấu trúc cây lecture";
            }

            return response;
        }

        // Helper method - Xây dựng cấu trúc cây
        private void BuildLectureTree(LectureTreeDto parent, List<Lecture> allLectures)
        {
            var children = allLectures
                .Where(l => l.ParentLectureId == parent.LectureId)
                .OrderBy(l => l.OrderIndex)
                .ToList();

            foreach (var child in children)
            {
                var childDto = _mapper.Map<LectureTreeDto>(child);
                parent.Children.Add(childDto);
                BuildLectureTree(childDto, allLectures);
            }
        }
    }
}
