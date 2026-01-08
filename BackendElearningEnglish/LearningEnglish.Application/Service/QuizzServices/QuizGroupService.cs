using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service
{
    public class QuizGroupService : IQuizGroupService
    {
        private readonly IQuizGroupRepository _quizGroupRepository;
        private readonly IMapper _mapper;
        private readonly IMinioFileStorage _minioFileStorage;

        // MinIO bucket constants
        private const string QuizGroupBucket = "quizgroups";
        private const string QuizGroupFolder = "real";

        public QuizGroupService(
            IQuizGroupRepository quizGroupRepository,
            IMapper mapper,
            IMinioFileStorage minioFileStorage)
        {
            _quizGroupRepository = quizGroupRepository;
            _mapper = mapper;
            _minioFileStorage = minioFileStorage;
        }

        public async Task<ServiceResponse<QuizGroupDto>> CreateQuizGroupAsync(CreateQuizGroupDto createDto)
        {
            var response = new ServiceResponse<QuizGroupDto>();

            try
            {
                // Check if quiz section exists
                var quizSection = await _quizGroupRepository.GetQuizSectionByIdAsync(createDto.QuizSectionId);
                if (quizSection == null)
                {
                    response.Success = false;
                    response.Message = "Quiz section không tồn tại.";
                    return response;
                }

                var quizGroup = _mapper.Map<QuizGroup>(createDto);
                string? committedImgKey = null;
                string? committedVideoKey = null;

                // Commit ImgTempKey nếu có
                if (!string.IsNullOrWhiteSpace(createDto.ImgTempKey))
                {
                    var imgResult = await _minioFileStorage.CommitFileAsync(
                        createDto.ImgTempKey,
                        QuizGroupBucket,
                        QuizGroupFolder
                    );

                    if (!imgResult.Success || string.IsNullOrWhiteSpace(imgResult.Data))
                    {
                        response.Success = false;
                        response.Message = $"Không thể lưu ảnh: {imgResult.Message}";
                        return response;
                    }

                    committedImgKey = imgResult.Data;
                    quizGroup.ImgKey = committedImgKey;
                }

                // Commit VideoTempKey nếu có
                if (!string.IsNullOrWhiteSpace(createDto.VideoTempKey))
                {
                    var videoResult = await _minioFileStorage.CommitFileAsync(
                        createDto.VideoTempKey,
                        QuizGroupBucket,
                        QuizGroupFolder
                    );

                    if (!videoResult.Success || string.IsNullOrWhiteSpace(videoResult.Data))
                    {
                        // Rollback img nếu đã commit
                        if (committedImgKey != null)
                        {
                            await _minioFileStorage.DeleteFileAsync(committedImgKey, QuizGroupBucket);
                        }

                        response.Success = false;
                        response.Message = $"Không thể lưu video: {videoResult.Message}";
                        return response;
                    }

                    committedVideoKey = videoResult.Data;
                    quizGroup.VideoKey = committedVideoKey;
                }

                // Save to database with rollback
                QuizGroup createdQuizGroup;
                try
                {
                    createdQuizGroup = await _quizGroupRepository.CreateQuizGroupAsync(quizGroup);
                }
                catch (Exception dbEx)
                {
                    // Rollback MinIO files
                    if (committedImgKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(committedImgKey, QuizGroupBucket);
                    }
                    if (committedVideoKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(committedVideoKey, QuizGroupBucket);
                    }

                    response.Success = false;
                    response.Message = $"Lỗi database: {dbEx.Message}";
                    return response;
                }

                var quizGroupDto = _mapper.Map<QuizGroupDto>(createdQuizGroup);

                // Generate URLs cho response
                if (!string.IsNullOrWhiteSpace(quizGroupDto.ImgUrl))
                {
                    quizGroupDto.ImgUrl = BuildPublicUrl.BuildURL(QuizGroupBucket, quizGroupDto.ImgUrl);
                }
                if (!string.IsNullOrWhiteSpace(quizGroupDto.VideoUrl))
                {
                    quizGroupDto.VideoUrl = BuildPublicUrl.BuildURL(QuizGroupBucket, quizGroupDto.VideoUrl);
                }

                response.Data = quizGroupDto;
                response.Success = true;
                response.Message = "Tạo nhóm câu hỏi thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi tạo nhóm câu hỏi: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<QuizGroupDto>> GetQuizGroupByIdAsync(int quizGroupId)
        {
            var response = new ServiceResponse<QuizGroupDto>();

            try
            {
                var quizGroup = await _quizGroupRepository.GetQuizGroupByIdAsync(quizGroupId);
                if (quizGroup == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy nhóm câu hỏi.";
                    return response;
                }

                var quizGroupDto = _mapper.Map<QuizGroupDto>(quizGroup);

                // Generate URLs từ keys
                if (!string.IsNullOrWhiteSpace(quizGroupDto.ImgUrl))
                {
                    quizGroupDto.ImgUrl = BuildPublicUrl.BuildURL(QuizGroupBucket, quizGroupDto.ImgUrl);
                }
                if (!string.IsNullOrWhiteSpace(quizGroupDto.VideoUrl))
                {
                    quizGroupDto.VideoUrl = BuildPublicUrl.BuildURL(QuizGroupBucket, quizGroupDto.VideoUrl);
                }

                response.Data = quizGroupDto;
                response.Success = true;
                response.Message = "Lấy thông tin nhóm câu hỏi thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi lấy thông tin nhóm câu hỏi: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<List<QuizGroupDto>>> GetQuizGroupsByQuizSectionIdAsync(int quizSectionId)
        {
            var response = new ServiceResponse<List<QuizGroupDto>>();

            try
            {
                var quizGroups = await _quizGroupRepository.GetQuizGroupsByQuizSectionIdAsync(quizSectionId);
                var quizGroupDtos = _mapper.Map<List<QuizGroupDto>>(quizGroups);

                // Generate URLs cho tất cả quiz groups
                foreach (var dto in quizGroupDtos)
                {
                    if (!string.IsNullOrWhiteSpace(dto.ImgUrl))
                    {
                        dto.ImgUrl = BuildPublicUrl.BuildURL(QuizGroupBucket, dto.ImgUrl);
                    }
                    if (!string.IsNullOrWhiteSpace(dto.VideoUrl))
                    {
                        dto.VideoUrl = BuildPublicUrl.BuildURL(QuizGroupBucket, dto.VideoUrl);
                    }
                }

                response.Data = quizGroupDtos;
                response.Success = true;
                response.Message = "Lấy danh sách nhóm câu hỏi thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi lấy danh sách nhóm câu hỏi: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<QuizGroupDto>> UpdateQuizGroupAsync(int quizGroupId, UpdateQuizGroupDto updateDto)
        {
            var response = new ServiceResponse<QuizGroupDto>();

            try
            {
                var existingQuizGroup = await _quizGroupRepository.GetQuizGroupByIdAsync(quizGroupId);
                if (existingQuizGroup == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy nhóm câu hỏi.";
                    return response;
                }

                // Update properties
                existingQuizGroup.Name = updateDto.Name;
                existingQuizGroup.Description = updateDto.Description;
                existingQuizGroup.Title = updateDto.Title;
                existingQuizGroup.ImgType = updateDto.ImgType;
                existingQuizGroup.VideoType = updateDto.VideoType;
                existingQuizGroup.VideoDuration = updateDto.VideoDuration;
                existingQuizGroup.SumScore = updateDto.SumScore;

                string? newImgKey = null;
                string? newVideoKey = null;
                string? oldImgKey = existingQuizGroup.ImgKey;
                string? oldVideoKey = existingQuizGroup.VideoKey;

                // Xử lý cập nhật ImgUrl
                if (!string.IsNullOrWhiteSpace(updateDto.ImgTempKey))
                {
                    var imgResult = await _minioFileStorage.CommitFileAsync(
                        updateDto.ImgTempKey,
                        QuizGroupBucket,
                        QuizGroupFolder
                    );

                    if (!imgResult.Success || string.IsNullOrWhiteSpace(imgResult.Data))
                    {
                        response.Success = false;
                        response.Message = $"Không thể lưu ảnh mới: {imgResult.Message}";
                        return response;
                    }

                    newImgKey = imgResult.Data;
                    existingQuizGroup.ImgKey = newImgKey;
                }

                // Xử lý cập nhật VideoUrl
                if (!string.IsNullOrWhiteSpace(updateDto.VideoTempKey))
                {
                    var videoResult = await _minioFileStorage.CommitFileAsync(
                        updateDto.VideoTempKey,
                        QuizGroupBucket,
                        QuizGroupFolder
                    );

                    if (!videoResult.Success || string.IsNullOrWhiteSpace(videoResult.Data))
                    {
                        // Rollback img if committed
                        if (newImgKey != null)
                        {
                            await _minioFileStorage.DeleteFileAsync(newImgKey, QuizGroupBucket);
                            existingQuizGroup.ImgKey = oldImgKey;
                        }

                        response.Success = false;
                        response.Message = $"Không thể lưu video mới: {videoResult.Message}";
                        return response;
                    }

                    newVideoKey = videoResult.Data;
                    existingQuizGroup.VideoKey = newVideoKey;
                }

                // Update database with rollback
                QuizGroup updatedQuizGroup;
                try
                {
                    updatedQuizGroup = await _quizGroupRepository.UpdateQuizGroupAsync(existingQuizGroup);
                }
                catch (Exception dbEx)
                {
                    // Rollback new files
                    if (newImgKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(newImgKey, QuizGroupBucket);
                    }
                    if (newVideoKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(newVideoKey, QuizGroupBucket);
                    }

                    response.Success = false;
                    response.Message = $"Lỗi database: {dbEx.Message}";
                    return response;
                }

                // Only delete old files after successful DB update
                if (newImgKey != null && !string.IsNullOrWhiteSpace(oldImgKey))
                {
                    await _minioFileStorage.DeleteFileAsync(oldImgKey, QuizGroupBucket);
                }
                if (newVideoKey != null && !string.IsNullOrWhiteSpace(oldVideoKey))
                {
                    await _minioFileStorage.DeleteFileAsync(oldVideoKey, QuizGroupBucket);
                }

                var quizGroupDto = _mapper.Map<QuizGroupDto>(updatedQuizGroup);

                // Generate URLs cho response
                if (!string.IsNullOrWhiteSpace(quizGroupDto.ImgUrl))
                {
                    quizGroupDto.ImgUrl = BuildPublicUrl.BuildURL(QuizGroupBucket, quizGroupDto.ImgUrl);
                }
                if (!string.IsNullOrWhiteSpace(quizGroupDto.VideoUrl))
                {
                    quizGroupDto.VideoUrl = BuildPublicUrl.BuildURL(QuizGroupBucket, quizGroupDto.VideoUrl);
                }

                response.Data = quizGroupDto;
                response.Success = true;
                response.Message = "Cập nhật nhóm câu hỏi thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi cập nhật nhóm câu hỏi: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteQuizGroupAsync(int quizGroupId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var quizGroup = await _quizGroupRepository.GetQuizGroupByIdAsync(quizGroupId);
                if (quizGroup == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy nhóm câu hỏi.";
                    return response;
                }

                // Check if quiz group has questions
                if (quizGroup.Questions?.Any() == true)
                {
                    response.Success = false;
                    response.Message = "Không thể xóa nhóm câu hỏi đã có câu hỏi. Vui lòng xóa các câu hỏi trước.";
                    return response;
                }

                // Xóa ảnh từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(quizGroup.ImgKey))
                {
                    await _minioFileStorage.DeleteFileAsync(QuizGroupBucket, quizGroup.ImgKey);
                }

                // Xóa video từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(quizGroup.VideoKey))
                {
                    await _minioFileStorage.DeleteFileAsync(QuizGroupBucket, quizGroup.VideoKey);
                }

                var deleted = await _quizGroupRepository.DeleteQuizGroupAsync(quizGroupId);
                if (!deleted)
                {
                    response.Success = false;
                    response.Message = "Không thể xóa nhóm câu hỏi.";
                    return response;
                }

                response.Data = true;
                response.Success = true;
                response.Message = "Xóa nhóm câu hỏi thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi xóa nhóm câu hỏi: {ex.Message}";
            }

            return response;
        }
    }
}
