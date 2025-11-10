using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOS;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class LectureService : ILectureService
    {
        private readonly ILectureRepository _lectureRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LectureService> _logger;

        public LectureService(
            ILectureRepository lectureRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<LectureService> logger)
        {
            _lectureRepository = lectureRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // + Kiểm tra quyền teacher với lecture
        public async Task<bool> CheckTeacherLecturePermission(int lectureId, int teacherId)
        {
            try
            {
                var lecture = await _lectureRepository.GetLectureWithModuleCourseAsync(lectureId);
                if (lecture?.Module?.Lesson?.Course == null) return false;

                var course = lecture.Module.Lesson.Course;
                return course.Type == CourseType.Teacher && course.TeacherId == teacherId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra quyền teacher với lecture: {LectureId}, Teacher: {TeacherId}", lectureId, teacherId);
                return false;
            }
        }

        // + Lấy thông tin lecture theo ID
        public async Task<ServiceResponse<LectureDto>> GetLectureByIdAsync(int lectureId, int? userId = null)
        {
            var response = new ServiceResponse<LectureDto>();

            try
            {
                var lecture = await _lectureRepository.GetByIdWithDetailsAsync(lectureId);
                if (lecture == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy lecture";
                    return response;
                }

                var lectureDto = _mapper.Map<LectureDto>(lecture);
                response.Data = lectureDto;
                response.Message = "Lấy thông tin lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lecture với ID: {LectureId}", lectureId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi lấy thông tin lecture";
            }

            return response;
        }

        // + Lấy danh sách lecture theo module
        public async Task<ServiceResponse<List<ListLectureDto>>> GetLecturesByModuleIdAsync(int moduleId, int? userId = null)
        {
            var response = new ServiceResponse<List<ListLectureDto>>();

            try
            {
                var lectures = await _lectureRepository.GetByModuleIdWithDetailsAsync(moduleId);
                var lectureDtos = _mapper.Map<List<ListLectureDto>>(lectures);

                response.Data = lectureDtos;
                response.Message = $"Lấy danh sách {lectures.Count} lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lecture theo ModuleId: {ModuleId}", moduleId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi lấy danh sách lecture";
            }

            return response;
        }

        // + Lấy cấu trúc cây lecture theo module
        public async Task<ServiceResponse<List<LectureTreeDto>>> GetLectureTreeByModuleIdAsync(int moduleId, int? userId = null)
        {
            var response = new ServiceResponse<List<LectureTreeDto>>();

            try
            {
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

                response.Data = treeDtos;
                response.Message = "Lấy cấu trúc cây lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cấu trúc cây lecture theo ModuleId: {ModuleId}", moduleId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi lấy cấu trúc cây lecture";
            }

            return response;
        }

        // + Tạo lecture mới
        public async Task<ServiceResponse<LectureDto>> CreateLectureAsync(CreateLectureDto createLectureDto, int createdByUserId)
        {
            var response = new ServiceResponse<LectureDto>();

            try
            {
                // Tự động set OrderIndex nếu không có
                if (createLectureDto.OrderIndex == 0)
                {
                    var maxOrder = await _lectureRepository.GetMaxOrderIndexAsync(createLectureDto.ModuleId, createLectureDto.ParentLectureId);
                    createLectureDto.OrderIndex = maxOrder + 1;
                }

                // Kiểm tra parent hợp lệ nếu có
                if (createLectureDto.ParentLectureId.HasValue)
                {
                    // Tạo lecture tạm để kiểm tra
                    var tempLecture = new Lecture { ModuleId = createLectureDto.ModuleId };
                    var isValidParent = await _lectureRepository.IsValidParentAsync(0, createLectureDto.ParentLectureId);
                    
                    if (!isValidParent)
                    {
                        response.Success = false;
                        response.Message = "Parent lecture không hợp lệ";
                        return response;
                    }
                }

                var lecture = _mapper.Map<Lecture>(createLectureDto);
                
                // Render HTML từ Markdown (đơn giản)
                if (!string.IsNullOrEmpty(lecture.MarkdownContent))
                {
                    lecture.RenderedHtml = ConvertMarkdownToHtml(lecture.MarkdownContent);
                }

                var createdLecture = await _lectureRepository.CreateAsync(lecture);
                var lectureDto = _mapper.Map<LectureDto>(createdLecture);

                response.Data = lectureDto;
                response.Message = "Tạo lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lecture mới: {LectureTitle}", createLectureDto.Title);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi tạo lecture";
            }

            return response;
        }

        // + Cập nhật lecture
        public async Task<ServiceResponse<LectureDto>> UpdateLectureAsync(int lectureId, UpdateLectureDto updateLectureDto, int updatedByUserId)
        {
            var response = new ServiceResponse<LectureDto>();

            try
            {
                var existingLecture = await _lectureRepository.GetByIdAsync(lectureId);
                if (existingLecture == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy lecture";
                    return response;
                }

                // Kiểm tra parent hợp lệ nếu có thay đổi
                if (updateLectureDto.ParentLectureId.HasValue && updateLectureDto.ParentLectureId != existingLecture.ParentLectureId)
                {
                    var isValidParent = await _lectureRepository.IsValidParentAsync(lectureId, updateLectureDto.ParentLectureId);
                    if (!isValidParent)
                    {
                        response.Success = false;
                        response.Message = "Parent lecture không hợp lệ";
                        return response;
                    }
                }

                // Cập nhật các trường được gửi lên
                _mapper.Map(updateLectureDto, existingLecture);

                // Re-render HTML nếu có thay đổi MarkdownContent
                if (!string.IsNullOrEmpty(existingLecture.MarkdownContent))
                {
                    existingLecture.RenderedHtml = ConvertMarkdownToHtml(existingLecture.MarkdownContent);
                }

                var updatedLecture = await _lectureRepository.UpdateAsync(existingLecture);
                var lectureDto = _mapper.Map<LectureDto>(updatedLecture);

                response.Data = lectureDto;
                response.Message = "Cập nhật lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật lecture với ID: {LectureId}", lectureId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi cập nhật lecture";
            }

            return response;
        }

        // + Xóa lecture
        public async Task<ServiceResponse<bool>> DeleteLectureAsync(int lectureId, int deletedByUserId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Kiểm tra lecture có tồn tại
                var exists = await _lectureRepository.ExistsAsync(lectureId);
                if (!exists)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy lecture";
                    return response;
                }

                // Kiểm tra có lecture con không
                var hasChildren = await _lectureRepository.HasChildrenAsync(lectureId);
                if (hasChildren)
                {
                    response.Success = false;
                    response.Message = "Không thể xóa lecture có lecture con. Vui lòng xóa các lecture con trước";
                    return response;
                }

                var deleted = await _lectureRepository.DeleteAsync(lectureId);
                response.Data = deleted;
                response.Message = deleted ? "Xóa lecture thành công" : "Không thể xóa lecture";
                response.Success = deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa lecture với ID: {LectureId}", lectureId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi xóa lecture";
            }

            return response;
        }

        // + Cập nhật lecture với authorization
        public async Task<ServiceResponse<LectureDto>> UpdateLectureWithAuthorizationAsync(int lectureId, UpdateLectureDto updateLectureDto, int userId, string userRole)
        {
            var response = new ServiceResponse<LectureDto>();

            try
            {
                // Admin có thể cập nhật tất cả
                if (userRole == "Admin")
                {
                    return await UpdateLectureAsync(lectureId, updateLectureDto, userId);
                }

                // Teacher chỉ có thể cập nhật lecture của mình
                if (userRole == "Teacher")
                {
                    var hasPermission = await CheckTeacherLecturePermission(lectureId, userId);
                    if (!hasPermission)
                    {
                        response.Success = false;
                        response.Message = "Bạn không có quyền cập nhật lecture này";
                        return response;
                    }

                    return await UpdateLectureAsync(lectureId, updateLectureDto, userId);
                }

                response.Success = false;
                response.Message = "Bạn không có quyền cập nhật lecture";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật lecture với authorization: LectureId={LectureId}, UserId={UserId}, Role={UserRole}", lectureId, userId, userRole);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi cập nhật lecture";
            }

            return response;
        }

        // + Xóa lecture với authorization
        public async Task<ServiceResponse<bool>> DeleteLectureWithAuthorizationAsync(int lectureId, int userId, string userRole)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Admin có thể xóa tất cả
                if (userRole == "Admin")
                {
                    return await DeleteLectureAsync(lectureId, userId);
                }

                // Teacher chỉ có thể xóa lecture của mình
                if (userRole == "Teacher")
                {
                    var hasPermission = await CheckTeacherLecturePermission(lectureId, userId);
                    if (!hasPermission)
                    {
                        response.Success = false;
                        response.Message = "Bạn không có quyền xóa lecture này";
                        return response;
                    }

                    return await DeleteLectureAsync(lectureId, userId);
                }

                response.Success = false;
                response.Message = "Bạn không có quyền xóa lecture";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa lecture với authorization: LectureId={LectureId}, UserId={UserId}, Role={UserRole}", lectureId, userId, userRole);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi xóa lecture";
            }

            return response;
        }

        // + Sắp xếp lại lecture
        public async Task<ServiceResponse<bool>> ReorderLecturesAsync(List<ReorderLectureDto> reorderDtos, int userId, string userRole)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                foreach (var reorderDto in reorderDtos)
                {
                    var lecture = await _lectureRepository.GetByIdAsync(reorderDto.LectureId);
                    if (lecture == null) continue;

                    // Kiểm tra quyền nếu là Teacher
                    if (userRole == "Teacher")
                    {
                        var hasPermission = await CheckTeacherLecturePermission(reorderDto.LectureId, userId);
                        if (!hasPermission) continue;
                    }

                    lecture.OrderIndex = reorderDto.NewOrderIndex;
                    if (reorderDto.NewParentLectureId.HasValue)
                    {
                        lecture.ParentLectureId = reorderDto.NewParentLectureId;
                    }

                    await _lectureRepository.UpdateAsync(lecture);
                }

                response.Data = true;
                response.Message = "Sắp xếp lại lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi sắp xếp lại lecture: UserId={UserId}, Role={UserRole}", userId, userRole);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi sắp xếp lại lecture";
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

        // Helper method - Convert Markdown to HTML (đơn giản)
        private string ConvertMarkdownToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return "";

            // Đây là implementation đơn giản, trong thực tế nên dùng thư viện như Markdig
            return markdown
                .Replace("\n", "<br>")
                .Replace("**", "<strong>", StringComparison.OrdinalIgnoreCase)
                .Replace("**", "</strong>", StringComparison.OrdinalIgnoreCase);
        }
    }
}
