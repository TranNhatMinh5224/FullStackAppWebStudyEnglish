using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Lecture;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AdminLectureService : IAdminLectureService
    {
        private readonly ILectureRepository _lectureRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminLectureService> _logger;
        private readonly IMinioFileStorage _minioFileStorage;

        // Đặt bucket + folder cho media lecture (video, audio, etc.)
        private const string LectureMediaBucket = "lectures";
        private const string LectureMediaFolder = "real";

        public AdminLectureService(
            ILectureRepository lectureRepository,
            IMapper mapper,
            ILogger<AdminLectureService> logger,
            IMinioFileStorage minioFileStorage)
        {
            _lectureRepository = lectureRepository;
            _mapper = mapper;
            _logger = logger;
            _minioFileStorage = minioFileStorage;
        }

        // Admin tạo lecture
       
        public async Task<ServiceResponse<LectureDto>> AdminCreateLecture(CreateLectureDto createLectureDto)
        {
            var response = new ServiceResponse<LectureDto>();

            _logger.LogInformation("Admin đang tạo lecture. ModuleId: {ModuleId}, ParentLectureId: {ParentId}, Title: {Title}",
                createLectureDto.ModuleId, createLectureDto.ParentLectureId, createLectureDto.Title);

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
                    _logger.LogInformation("Checking parent lecture with ID: {ParentId}", createLectureDto.ParentLectureId.Value);

                    // Kiểm tra parent lecture có tồn tại không
                    var parentLecture = await _lectureRepository.GetByIdAsync(createLectureDto.ParentLectureId.Value);

                    if (parentLecture == null)
                    {
                        _logger.LogWarning("Parent lecture not found with ID: {ParentId}", createLectureDto.ParentLectureId.Value);
                        response.Success = false;
                        response.Message = "Parent lecture không tồn tại";
                        return response;
                    }

                    _logger.LogInformation("Parent lecture found. ParentModuleId: {ParentModuleId}, CurrentModuleId: {CurrentModuleId}",
                        parentLecture.ModuleId, createLectureDto.ModuleId);

                    // Kiểm tra parent và lecture mới phải cùng module
                    if (parentLecture.ModuleId != createLectureDto.ModuleId)
                    {
                        _logger.LogWarning("Module mismatch. Parent ModuleId: {ParentModuleId}, Current ModuleId: {CurrentModuleId}",
                            parentLecture.ModuleId, createLectureDto.ModuleId);
                        response.Success = false;
                        response.Message = "Parent lecture phải thuộc cùng module";
                        return response;
                    }

                    _logger.LogInformation("Parent validation passed successfully");
                }

                var lecture = _mapper.Map<Lecture>(createLectureDto);

                string? committedMediaKey = null;

                // Commit MediaTempKey nếu có
                if (!string.IsNullOrWhiteSpace(createLectureDto.MediaTempKey))
                {
                    var mediaResult = await _minioFileStorage.CommitFileAsync(
                        createLectureDto.MediaTempKey,
                        LectureMediaBucket,
                        LectureMediaFolder
                    );

                    if (!mediaResult.Success || string.IsNullOrWhiteSpace(mediaResult.Data))
                    {
                        _logger.LogError("Failed to commit lecture media: {Error}", mediaResult.Message);
                        response.Success = false;
                        response.Message = $"Không thể lưu media: {mediaResult.Message}";
                        return response;
                    }

                    committedMediaKey = mediaResult.Data;
                    lecture.MediaKey = committedMediaKey;
                }

                Lecture createdLecture;
                try
                {
                    createdLecture = await _lectureRepository.CreateAsync(lecture);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while creating lecture");

                    // Rollback MinIO file
                    if (committedMediaKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(committedMediaKey, LectureMediaBucket);
                    }

                    response.Success = false;
                    response.Message = "Lỗi database khi tạo lecture";
                    return response;
                }
                
                var lectureDto = _mapper.Map<LectureDto>(createdLecture);

                // Generate URL cho response
                if (!string.IsNullOrWhiteSpace(lectureDto.MediaUrl))
                {
                    lectureDto.MediaUrl = BuildPublicUrl.BuildURL(LectureMediaBucket, lectureDto.MediaUrl);
                }

                response.Data = lectureDto;
                response.Message = "Tạo lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Admin tạo lecture: {LectureTitle}", createLectureDto.Title);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi tạo lecture";
            }

            return response;
        }

        // Admin tạo nhiều lectures cùng lúc với cấu trúc cha-con (Bulk Create)
       
        public async Task<ServiceResponse<BulkCreateLecturesResponseDto>> AdminBulkCreateLectures(BulkCreateLecturesDto bulkCreateDto)
        {
            var response = new ServiceResponse<BulkCreateLecturesResponseDto>
            {
                Data = new BulkCreateLecturesResponseDto()
            };

            _logger.LogInformation("Admin đang bulk create lectures. ModuleId: {ModuleId}, TotalLectures: {Count}",
                bulkCreateDto.ModuleId, bulkCreateDto.Lectures.Count);

            try
            {
                // Validation: Check if module exists by trying to get existing lectures
                // If no error thrown, module exists (even if no lectures yet)
                var existingLectures = await _lectureRepository.GetByModuleIdAsync(bulkCreateDto.ModuleId);
                
                _logger.LogInformation("Found {Count} existing lectures in module {ModuleId}", 
                    existingLectures.Count, bulkCreateDto.ModuleId);

                // Validation: Check for duplicate TempIds
                var tempIds = bulkCreateDto.Lectures.Select(l => l.TempId).ToList();
                var duplicates = tempIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicates.Any())
                {
                    response.Success = false;
                    response.Message = $"TempId bị trùng lặp: {string.Join(", ", duplicates)}";
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
                    response.Data.Errors.Add($"ParentTempId không hợp lệ: {string.Join(", ", invalidParents)}");
                    return response;
                }

                // Map TempId → LectureEntity (chưa có ID thật)
                var tempIdToLecture = new Dictionary<string, Lecture>();
                var tempIdToMediaKey = new Dictionary<string, string>(); // Track committed media keys for rollback
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
                        MediaType = lectureNode.MediaType,
                        MediaSize = lectureNode.MediaSize,
                        Duration = lectureNode.Duration,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ParentLectureId = null // Will be set after parents are created
                    };

                    tempIdToLecture[lectureNode.TempId] = lecture;
                }

                // Topological sort to create parents before children
                var sortedLectures = TopologicalSort(bulkCreateDto.Lectures);
                if (sortedLectures == null)
                {
                    // Rollback media
                    foreach (var key in committedMediaKeys)
                    {
                        await _minioFileStorage.DeleteFileAsync(key, LectureMediaBucket);
                    }

                    response.Success = false;
                    response.Message = "Phát hiện circular dependency trong cấu trúc cha-con";
                    response.Data.Errors.Add("Circular dependency detected");
                    return response;
                }

                // Create lectures in order (parents first) using transaction
                var tempIdToRealId = new Dictionary<string, int>();

                try
                {
                    foreach (var lectureNode in sortedLectures)
                    {
                        var lecture = tempIdToLecture[lectureNode.TempId];

                        // Set ParentLectureId if parent exists
                        if (!string.IsNullOrEmpty(lectureNode.ParentTempId))
                        {
                            if (tempIdToRealId.ContainsKey(lectureNode.ParentTempId))
                            {
                                lecture.ParentLectureId = tempIdToRealId[lectureNode.ParentTempId];
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"Parent lecture with TempId '{lectureNode.ParentTempId}' has not been created yet");
                            }
                        }

                        // Save to database
                        var createdLecture = await _lectureRepository.CreateAsync(lecture);
                        tempIdToRealId[lectureNode.TempId] = createdLecture.LectureId;

                        // Map to DTO for response
                        var lectureDto = _mapper.Map<LectureDto>(createdLecture);
                        if (!string.IsNullOrWhiteSpace(lectureDto.MediaUrl))
                        {
                            lectureDto.MediaUrl = BuildPublicUrl.BuildURL(LectureMediaBucket, lectureDto.MediaUrl);
                        }

                        response.Data.CreatedLectures[lectureNode.TempId] = lectureDto;
                        response.Data.TotalCreated++;

                        _logger.LogInformation("Created lecture with TempId: {TempId}, RealId: {RealId}",
                            lectureNode.TempId, createdLecture.LectureId);
                    }

                    response.Success = true;
                    response.Message = $"Tạo thành công {response.Data.TotalCreated} lectures";
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error during bulk create");

                    // Rollback: Delete all committed media files
                    foreach (var key in committedMediaKeys)
                    {
                        try
                        {
                            await _minioFileStorage.DeleteFileAsync(key, LectureMediaBucket);
                        }
                        catch (Exception cleanupEx)
                        {
                            _logger.LogWarning(cleanupEx, "Failed to cleanup media file: {Key}", key);
                        }
                    }

                    // Note: Database rollback is handled by transaction (if using one)
                    // If not using explicit transaction, created lectures will remain in DB
                    response.Success = false;
                    response.Message = "Lỗi database khi tạo lectures. Đã rollback media files.";
                    response.Data.Errors.Add($"Database error: {dbEx.Message}");
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AdminBulkCreateLectures");
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi tạo bulk lectures";
                response.Data.Errors.Add(ex.Message);
            }

            return response;
        }

        // Cập nhật lecture
               public async Task<ServiceResponse<LectureDto>> UpdateLecture(int lectureId, UpdateLectureDto updateLectureDto)
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
                    // Không thể tự trỏ vào chính mình
                    if (updateLectureDto.ParentLectureId.Value == lectureId)
                    {
                        response.Success = false;
                        response.Message = "Lecture không thể là parent của chính mình";
                        return response;
                    }

                    // Kiểm tra parent lecture có tồn tại không
                    var parentLecture = await _lectureRepository.GetByIdAsync(updateLectureDto.ParentLectureId.Value);
                    if (parentLecture == null)
                    {
                        response.Success = false;
                        response.Message = "Parent lecture không tồn tại";
                        return response;
                    }

                    // Kiểm tra parent và lecture phải cùng module
                    if (parentLecture.ModuleId != existingLecture.ModuleId)
                    {
                        response.Success = false;
                        response.Message = "Parent lecture phải thuộc cùng module";
                        return response;
                    }
                }

                // Cập nhật các trường được gửi lên
                _mapper.Map(updateLectureDto, existingLecture);

                string? newMediaKey = null;
                string? oldMediaKey = !string.IsNullOrWhiteSpace(existingLecture.MediaKey) ? existingLecture.MediaKey : null;

                // Xử lý cập nhật MediaUrl
                if (!string.IsNullOrWhiteSpace(updateLectureDto.MediaTempKey))
                {
                    // Commit media mới
                    var mediaResult = await _minioFileStorage.CommitFileAsync(
                        updateLectureDto.MediaTempKey,
                        LectureMediaBucket,
                        LectureMediaFolder
                    );

                    if (!mediaResult.Success || string.IsNullOrWhiteSpace(mediaResult.Data))
                    {
                        _logger.LogError("Failed to commit lecture media: {Error}", mediaResult.Message);
                        response.Success = false;
                        response.Message = $"Không thể lưu media: {mediaResult.Message}";
                        return response;
                    }

                    newMediaKey = mediaResult.Data;
                    existingLecture.MediaKey = newMediaKey;
                }

                Lecture updatedLecture;
                try
                {
                    updatedLecture = await _lectureRepository.UpdateAsync(existingLecture);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while updating lecture");

                    // Rollback new media
                    if (newMediaKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(newMediaKey, LectureMediaBucket);
                    }

                    response.Success = false;
                    response.Message = "Lỗi database khi cập nhật lecture";
                    return response;
                }

                // Delete old media only after successful DB update
                if (oldMediaKey != null && newMediaKey != null)
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(oldMediaKey, LectureMediaBucket);
                    }
                    catch
                    {
                        _logger.LogWarning("Failed to delete old lecture media: {MediaUrl}", oldMediaKey);
                    }
                }
                
                var lectureDto = _mapper.Map<LectureDto>(updatedLecture);

                // Generate URL cho response
                if (!string.IsNullOrWhiteSpace(lectureDto.MediaUrl))
                {
                    lectureDto.MediaUrl = BuildPublicUrl.BuildURL(LectureMediaBucket, lectureDto.MediaUrl);
                }

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

        // Xóa lecture
        // RLS: lectures_policy_* sẽ filter lectures theo role/permission khi DELETE
        // - Admin: Có thể xóa tất cả lectures (có permission)
        public async Task<ServiceResponse<bool>> DeleteLecture(int lectureId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Lấy lecture để check MediaUrl
                var lecture = await _lectureRepository.GetByIdAsync(lectureId);
                if (lecture == null)
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

                // Xóa media từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(lecture.MediaKey))
                {
                    await _minioFileStorage.DeleteFileAsync(lecture.MediaKey, LectureMediaBucket);
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

        // Sắp xếp lại lecture
        // RLS: lectures_policy_* sẽ filter lectures theo role/permission khi UPDATE
        // - Admin: Có thể reorder tất cả lectures (có permission)
        public async Task<ServiceResponse<bool>> ReorderLectures(List<ReorderLectureDto> reorderDtos)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                foreach (var reorderDto in reorderDtos)
                {
                    // RLS đã filter lectures theo role/permission
                    // Nếu lecture không tồn tại hoặc không có quyền → RLS sẽ filter → lecture == null
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

        // Admin lấy lecture theo ID (read-only)
        public async Task<ServiceResponse<LectureDto>> GetLectureByIdAsync(int lectureId)
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

                // Generate URL từ key cho MediaUrl
                if (!string.IsNullOrWhiteSpace(lectureDto.MediaUrl))
                {
                    lectureDto.MediaUrl = BuildPublicUrl.BuildURL(
                        LectureMediaBucket,
                        lectureDto.MediaUrl
                    );
                }

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

        // Admin lấy danh sách lecture theo module (read-only)
        public async Task<ServiceResponse<List<ListLectureDto>>> GetLecturesByModuleIdAsync(int moduleId)
        {
            var response = new ServiceResponse<List<ListLectureDto>>();

            try
            {
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

        // Admin lấy cây lecture (read-only)
        public async Task<ServiceResponse<List<LectureTreeDto>>> GetLectureTreeByModuleIdAsync(int moduleId)
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

        // Helper: Topological sort to ensure parents are created before children
        private List<LectureNodeDto>? TopologicalSort(List<LectureNodeDto> lectures)
        {
            var sorted = new List<LectureNodeDto>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            var lectureMap = lectures.ToDictionary(l => l.TempId);

            bool Visit(string tempId)
            {
                if (visited.Contains(tempId)) return true;
                if (visiting.Contains(tempId)) return false; // Circular dependency

                visiting.Add(tempId);

                var lecture = lectureMap[tempId];
                if (!string.IsNullOrEmpty(lecture.ParentTempId))
                {
                    if (!Visit(lecture.ParentTempId))
                        return false; // Circular dependency
                }

                visiting.Remove(tempId);
                visited.Add(tempId);
                sorted.Add(lecture);
                return true;
            }

            foreach (var lecture in lectures)
            {
                if (!Visit(lecture.TempId))
                    return null; // Circular dependency detected
            }

            return sorted;
        }
    }
}
