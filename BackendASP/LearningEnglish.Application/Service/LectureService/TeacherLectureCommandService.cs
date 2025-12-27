using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Lecture;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.LectureService
{
    public class TeacherLectureCommandService : ITeacherLectureCommandService
    {
        private readonly ILectureRepository _lectureRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherLectureCommandService> _logger;
        private readonly IMinioFileStorage _minioFileStorage;

        // Đặt bucket + folder cho media lecture (video, audio, etc.)
        private const string LectureMediaBucket = "lectures";
        private const string LectureMediaFolder = "real";

        public TeacherLectureCommandService(
            ILectureRepository lectureRepository,
            IModuleRepository moduleRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<TeacherLectureCommandService> logger,
            IMinioFileStorage minioFileStorage)
        {
            _lectureRepository = lectureRepository;
            _moduleRepository = moduleRepository;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
            _minioFileStorage = minioFileStorage;
        }

        // Teacher tạo lecture
        public async Task<ServiceResponse<LectureDto>> TeacherCreateLecture(CreateLectureDto createLectureDto, int teacherId)
        {
            var response = new ServiceResponse<LectureDto>();

            _logger.LogInformation("Teacher {TeacherId} đang tạo lecture. ModuleId: {ModuleId}, ParentLectureId: {ParentId}, Title: {Title}",
                teacherId, createLectureDto.ModuleId, createLectureDto.ParentLectureId, createLectureDto.Title);

            try
            {
                // Kiểm tra module ownership
                var module = await _moduleRepository.GetModuleWithCourseForTeacherAsync(createLectureDto.ModuleId, teacherId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module hoặc bạn không có quyền truy cập";
                    _logger.LogWarning("Teacher {TeacherId} attempted to create lecture for module {ModuleId} without ownership", 
                        teacherId, createLectureDto.ModuleId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được tạo lecture
                if (module.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể tạo lecture cho khóa học của giáo viên";
                    _logger.LogWarning("Teacher {TeacherId} attempted to create lecture for System course module {ModuleId}", 
                        teacherId, createLectureDto.ModuleId);
                    return response;
                }

                // Tự động set OrderIndex nếu không có
                if (createLectureDto.OrderIndex == 0)
                {
                    var maxOrder = await _lectureRepository.GetMaxOrderIndexAsync(createLectureDto.ModuleId, createLectureDto.ParentLectureId);
                    createLectureDto.OrderIndex = maxOrder + 1;
                }

                // Kiểm tra parent hợp lệ nếu có
                if (createLectureDto.ParentLectureId.HasValue)
                {
                    _logger.LogInformation("Checking parent lecture with ID: {ParentId}", createLectureDto.ParentLectureId.Value);

                    var parentLecture = await _lectureRepository.GetByIdAsync(createLectureDto.ParentLectureId.Value);
                    if (parentLecture == null)
                    {
                        _logger.LogWarning("Parent lecture not found with ID: {ParentId}", createLectureDto.ParentLectureId.Value);
                        response.Success = false;
                        response.Message = "Parent lecture không tồn tại";
                        return response;
                    }

                    if (parentLecture.ModuleId != createLectureDto.ModuleId)
                    {
                        _logger.LogWarning("Module mismatch. Parent ModuleId: {ParentModuleId}, Current ModuleId: {CurrentModuleId}",
                            parentLecture.ModuleId, createLectureDto.ModuleId);
                        response.Success = false;
                        response.Message = "Parent lecture phải thuộc cùng module";
                        return response;
                    }
                }

                var lecture = _mapper.Map<Lecture>(createLectureDto);

                string? committedMediaKey = null;
                if (!string.IsNullOrWhiteSpace(createLectureDto.MediaTempKey))
                {
                    var mediaResult = await _minioFileStorage.CommitFileAsync(
                        createLectureDto.MediaTempKey,
                        LectureMediaBucket,
                        LectureMediaFolder
                    );

                    if (!mediaResult.Success || string.IsNullOrWhiteSpace(mediaResult.Data))
                    {
                        _logger.LogError("Failed to commit media for lecture creation. Error: {Error}", mediaResult.Message);
                        response.Success = false;
                        response.Message = $"Không thể lưu media: {mediaResult.Message}";
                        return response;
                    }

                    committedMediaKey = mediaResult.Data;
                    lecture.MediaKey = committedMediaKey;
                }

                var createdLecture = await _lectureRepository.CreateAsync(lecture);

                var lectureDto = _mapper.Map<LectureDto>(createdLecture);

                // Generate URL từ key cho MediaUrl
                if (!string.IsNullOrWhiteSpace(lectureDto.MediaUrl))
                {
                    lectureDto.MediaUrl = BuildPublicUrl.BuildURL(
                        LectureMediaBucket,
                        lectureDto.MediaUrl
                    );
                }

                response.Data = lectureDto;
                response.Message = "Tạo lecture thành công";
                _logger.LogInformation("Teacher {TeacherId} đã tạo lecture {LectureId} thành công", teacherId, createdLecture.LectureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lecture cho teacher {TeacherId}", teacherId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi tạo lecture";
            }

            return response;
        }

        // Teacher tạo nhiều lecture cùng lúc
        public async Task<ServiceResponse<BulkCreateLecturesResponseDto>> TeacherBulkCreateLectures(BulkCreateLecturesDto bulkCreateDto, int teacherId)
        {
            var response = new ServiceResponse<BulkCreateLecturesResponseDto>();

            _logger.LogInformation("Teacher {TeacherId} đang bulk create lectures. ModuleId: {ModuleId}, TotalLectures: {Count}",
                teacherId, bulkCreateDto.ModuleId, bulkCreateDto.Lectures.Count);

            try
            {
                // Kiểm tra module ownership
                var module = await _moduleRepository.GetModuleWithCourseForTeacherAsync(bulkCreateDto.ModuleId, teacherId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module hoặc bạn không có quyền truy cập";
                    _logger.LogWarning("Teacher {TeacherId} attempted to bulk create lectures for module {ModuleId} without ownership", 
                        teacherId, bulkCreateDto.ModuleId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được tạo lecture
                if (module.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể tạo lecture cho khóa học của giáo viên";
                    _logger.LogWarning("Teacher {TeacherId} attempted to bulk create lectures for System course module {ModuleId}", 
                        teacherId, bulkCreateDto.ModuleId);
                    return response;
                }

                // Validation: Check if module exists by trying to get existing lectures
                var existingLectures = await _lectureRepository.GetByModuleIdAsync(bulkCreateDto.ModuleId);

                _logger.LogInformation("Found {Count} existing lectures in module {ModuleId}",
                    existingLectures.Count, bulkCreateDto.ModuleId);


                var tempIds = bulkCreateDto.Lectures.Select(l => l.TempId).ToList();
                var duplicates = tempIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicates.Any())
                {
                    response.Success = false;
                    response.Message = $"TempId bị trùng lặp: {string.Join(", ", duplicates)}";
                    response.Data ??= new BulkCreateLecturesResponseDto();
                    response.Data.Errors.Add($"TempId bị trùng lặp: {string.Join(", ", duplicates)}");
                    return response;
                }

                // Validation: Check if all ParentTempIds reference valid TempIds
                var invalidParents = bulkCreateDto.Lectures
                    .Where(l => !string.IsNullOrEmpty(l.ParentTempId) && !tempIds.Contains(l.ParentTempId))
                    .Select(l => l.TempId)
                    .ToList();

                if (invalidParents.Any())
                {
                    response.Success = false;
                    response.Message = $"ParentTempId không hợp lệ cho các lecture: {string.Join(", ", invalidParents)}";
                    response.Data ??= new BulkCreateLecturesResponseDto();
                    response.Data.Errors.Add($"ParentTempId không hợp lệ: {string.Join(", ", invalidParents)}");
                    return response;
                }

                // Map TempId → LectureEntity (chưa có ID thật)
                var tempIdToLecture = new Dictionary<string, Lecture>();
                var tempIdToMediaKey = new Dictionary<string, string>();
                var committedMediaKeys = new List<string>(); // For cleanup on error

                // Step 1: Process media uploads first (commit from temp to real)
                foreach (var lectureNode in bulkCreateDto.Lectures)
                {
                    string? committedMediaKey = null;

                    if (!string.IsNullOrWhiteSpace(lectureNode.MediaTempKey))
                    {
                        var mediaResult = await _minioFileStorage.CommitFileAsync(
                            lectureNode.MediaTempKey,
                            LectureMediaBucket,
                            LectureMediaFolder
                        );

                        if (!mediaResult.Success || string.IsNullOrWhiteSpace(mediaResult.Data))
                        {
                            _logger.LogError("Failed to commit media for TempId: {TempId}, Error: {Error}",
                                lectureNode.TempId, mediaResult.Message);

                            // Rollback all previously committed media
                            foreach (var key in committedMediaKeys)
                            {
                                await _minioFileStorage.DeleteFileAsync(key, LectureMediaBucket);
                            }

                            response.Success = false;
                            response.Message = $"Không thể lưu media cho lecture '{lectureNode.Title}': {mediaResult.Message}";
                            response.Data ??= new BulkCreateLecturesResponseDto();
                            response.Data.Errors.Add($"Media upload failed for {lectureNode.TempId}");
                            return response;
                        }

                        committedMediaKey = mediaResult.Data;
                        committedMediaKeys.Add(committedMediaKey);
                        tempIdToMediaKey[lectureNode.TempId] = committedMediaKey;
                    }

                    // Create Lecture entity
                    var lecture = new Lecture
                    {
                        ModuleId = bulkCreateDto.ModuleId,
                        OrderIndex = lectureNode.OrderIndex,
                        NumberingLabel = lectureNode.NumberingLabel,
                        Title = lectureNode.Title,
                        Type = lectureNode.Type,
                        MarkdownContent = lectureNode.MarkdownContent,
                        MediaKey = committedMediaKey,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    tempIdToLecture[lectureNode.TempId] = lecture;
                }

                // Step 2: Set parent relationships and save to database
                var createdLectures = new List<Lecture>();
                var tempIdToRealId = new Dictionary<string, int>();

                foreach (var lectureNode in bulkCreateDto.Lectures)
                {
                    var lecture = tempIdToLecture[lectureNode.TempId];

                    // Set parent relationship if exists
                    if (!string.IsNullOrEmpty(lectureNode.ParentTempId))
                    {
                        if (tempIdToRealId.ContainsKey(lectureNode.ParentTempId))
                        {
                            lecture.ParentLectureId = tempIdToRealId[lectureNode.ParentTempId];
                        }
                        else
                        {
                            // Parent should have been created already (topological order)
                            var parentLecture = createdLectures.FirstOrDefault(l => tempIdToLecture.FirstOrDefault(t => t.Value == l).Key == lectureNode.ParentTempId);
                            if (parentLecture != null)
                            {
                                lecture.ParentLectureId = parentLecture.LectureId;
                            }
                        }
                    }

                    var savedLecture = await _lectureRepository.CreateAsync(lecture);
                    createdLectures.Add(savedLecture);
                    tempIdToRealId[lectureNode.TempId] = savedLecture.LectureId;
                }

                // Step 3: Build response
                var responseLectures = createdLectures.Select(l =>
                {
                    var dto = _mapper.Map<LectureDto>(l);
                    if (!string.IsNullOrWhiteSpace(dto.MediaUrl))
                    {
                        dto.MediaUrl = BuildPublicUrl.BuildURL(LectureMediaBucket, dto.MediaUrl);
                    }
                    return dto;
                }).ToList();

                response.Data ??= new BulkCreateLecturesResponseDto();
                response.Data.CreatedLectures = responseLectures.ToDictionary(l => tempIdToRealId.First(t => t.Value == l.LectureId).Key, l => l);
                response.Data.TotalCreated = responseLectures.Count;
                response.Message = $"Tạo thành công {responseLectures.Count} lectures";
                _logger.LogInformation("Teacher {TeacherId} đã bulk create {Count} lectures thành công", teacherId, responseLectures.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi bulk create lectures cho teacher {TeacherId}", teacherId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi tạo nhiều lectures";
            }

            return response;
        }

        // Cập nhật lecture
        public async Task<ServiceResponse<LectureDto>> UpdateLecture(int lectureId, UpdateLectureDto updateLectureDto, int teacherId)
        {
            var response = new ServiceResponse<LectureDto>();

            try
            {
                var existingLecture = await _lectureRepository.GetLectureWithModuleCourseForTeacherAsync(lectureId, teacherId);
                if (existingLecture == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy lecture hoặc bạn không có quyền truy cập";
                    _logger.LogWarning("Teacher {TeacherId} attempted to update lecture {LectureId} without ownership", 
                        teacherId, lectureId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được cập nhật lecture
                if (existingLecture.Module?.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể cập nhật lecture của khóa học giáo viên";
                    _logger.LogWarning("Teacher {TeacherId} attempted to update lecture {LectureId} of System course", 
                        teacherId, lectureId);
                    return response;
                }

                // Load lecture để update
                var lectureToUpdate = await _lectureRepository.GetByIdAsync(lectureId);
                if (lectureToUpdate == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy lecture";
                    return response;
                }

                // Kiểm tra parent hợp lệ nếu có thay đổi
                if (updateLectureDto.ParentLectureId.HasValue && updateLectureDto.ParentLectureId != lectureToUpdate.ParentLectureId)
                {
                    // Không thể tự trỏ vào chính mình
                    if (updateLectureDto.ParentLectureId.Value == lectureId)
                    {
                        response.Success = false;
                        response.Message = "Lecture không thể là parent của chính mình";
                        return response;
                    }

                    var parentLecture = await _lectureRepository.GetByIdAsync(updateLectureDto.ParentLectureId.Value);
                    if (parentLecture == null)
                    {
                        response.Success = false;
                        response.Message = "Parent lecture không tồn tại";
                        return response;
                    }

                    if (parentLecture.ModuleId != lectureToUpdate.ModuleId)
                    {
                        response.Success = false;
                        response.Message = "Parent lecture phải thuộc cùng module";
                        return response;
                    }
                }

                // Handle media update
                string? committedMediaKey = lectureToUpdate.MediaKey;
                string? oldMediaKey = lectureToUpdate.MediaKey;
                
                if (!string.IsNullOrWhiteSpace(updateLectureDto.MediaTempKey))
                {
                    var mediaResult = await _minioFileStorage.CommitFileAsync(
                        updateLectureDto.MediaTempKey,
                        LectureMediaBucket,
                        LectureMediaFolder
                    );

                    if (!mediaResult.Success || string.IsNullOrWhiteSpace(mediaResult.Data))
                    {
                        _logger.LogError("Failed to commit media for lecture update. Error: {Error}", mediaResult.Message);
                        response.Success = false;
                        response.Message = $"Không thể lưu media: {mediaResult.Message}";
                        return response;
                    }

                    committedMediaKey = mediaResult.Data;
                }

                // Update lecture properties
                _mapper.Map(updateLectureDto, lectureToUpdate);
                lectureToUpdate.MediaKey = committedMediaKey;

                var updatedLecture = await _lectureRepository.UpdateAsync(lectureToUpdate);

                // Xóa media cũ chỉ sau khi update thành công
                if (!string.IsNullOrWhiteSpace(oldMediaKey) && committedMediaKey != null && oldMediaKey != committedMediaKey)
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(oldMediaKey, LectureMediaBucket);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to delete old lecture media: {MediaKey}", oldMediaKey);
                    }
                }

                var lectureDto = _mapper.Map<LectureDto>(updatedLecture);

                // Generate URL từ key cho MediaUrl
                if (!string.IsNullOrWhiteSpace(lectureDto.MediaUrl))
                {
                    lectureDto.MediaUrl = BuildPublicUrl.BuildURL(
                        LectureMediaBucket,
                        lectureDto.MediaUrl
                    );
                }

                response.Data = lectureDto;
                response.Message = "Cập nhật lecture thành công";
                _logger.LogInformation("Teacher {TeacherId} đã cập nhật lecture {LectureId} thành công", teacherId, lectureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật lecture {LectureId} cho teacher {TeacherId}", lectureId, teacherId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi cập nhật lecture";
            }

            return response;
        }

        // Xóa lecture
        public async Task<ServiceResponse<bool>> DeleteLecture(int lectureId, int teacherId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Lấy lecture để check ownership và MediaUrl
                var lecture = await _lectureRepository.GetLectureWithModuleCourseForTeacherAsync(lectureId, teacherId);
                if (lecture == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy lecture hoặc bạn không có quyền truy cập";
                    response.Data = false;
                    _logger.LogWarning("Teacher {TeacherId} attempted to delete lecture {LectureId} without ownership", 
                        teacherId, lectureId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được xóa lecture
                if (lecture.Module?.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể xóa lecture của khóa học giáo viên";
                    response.Data = false;
                    _logger.LogWarning("Teacher {TeacherId} attempted to delete lecture {LectureId} of System course", 
                        teacherId, lectureId);
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

                // Xóa media từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(lecture.MediaKey))
                {
                    var deleteResult = await _minioFileStorage.DeleteFileAsync(lecture.MediaKey, LectureMediaBucket);
                    if (!deleteResult.Success)
                    {
                        _logger.LogWarning("Failed to delete media file {MediaKey} for lecture {LectureId}", lecture.MediaKey, lectureId);
                        // Không return error, vẫn tiếp tục xóa lecture
                    }
                }

                var deleteResultDb = await _lectureRepository.DeleteAsync(lectureId);
                if (!deleteResultDb)
                {
                    response.Success = false;
                    response.Message = "Không thể xóa lecture";
                    return response;
                }

                response.Data = true;
                response.Message = "Xóa lecture thành công";
                _logger.LogInformation("Teacher {TeacherId} đã xóa lecture {LectureId} thành công", teacherId, lectureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa lecture {LectureId} cho teacher {TeacherId}", lectureId, teacherId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi xóa lecture";
            }

            return response;
        }

        // Sắp xếp lại lecture
        public async Task<ServiceResponse<bool>> ReorderLectures(List<ReorderLectureDto> reorderDtos, int teacherId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Kiểm tra tất cả lectures có thuộc về teacher không
                var lectureIds = reorderDtos.Select(r => r.LectureId).ToList();
                foreach (var lectureId in lectureIds)
                {
                    var lecture = await _lectureRepository.GetLectureWithModuleCourseForTeacherAsync(lectureId, teacherId);
                    if (lecture == null)
                    {
                        response.Success = false;
                        response.StatusCode = 404;
                        response.Message = $"Không tìm thấy lecture {lectureId} hoặc bạn không có quyền truy cập";
                        _logger.LogWarning("Teacher {TeacherId} attempted to reorder lecture {LectureId} without ownership", 
                            teacherId, lectureId);
                        return response;
                    }

                    // Business logic: Chỉ teacher course mới được sắp xếp lecture
                    if (lecture.Module?.Lesson?.Course?.Type != CourseType.Teacher)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = $"Không thể sắp xếp lecture {lectureId} của khóa học hệ thống";
                        _logger.LogWarning("Teacher {TeacherId} attempted to reorder lecture {LectureId} of System course", 
                            teacherId, lectureId);
                        return response;
                    }
                }

                foreach (var reorderDto in reorderDtos)
                {
                    var lecture = await _lectureRepository.GetByIdAsync(reorderDto.LectureId);
                    if (lecture == null) continue;

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
                _logger.LogError(ex, "Lỗi khi sắp xếp lại lecture");
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi sắp xếp lại lecture";
            }

            return response;
        }
    }
}