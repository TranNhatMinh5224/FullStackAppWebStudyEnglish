using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuizGroupRepository _quizGroupRepository;
        private readonly IQuizSectionRepository _quizSectionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestionService> _logger;
        private readonly IMinioFileStorage _minioFileStorage;
        
        // MinIO bucket constants
        private const string QuestionBucket = "questions";
        private const string QuestionFolder = "real";

        public QuestionService(
            IQuestionRepository questionRepository,
            IQuizGroupRepository quizGroupRepository,
            IQuizSectionRepository quizSectionRepository,
            IMapper mapper,
            ILogger<QuestionService> logger,
            IMinioFileStorage minioFileStorage)
        {
            _questionRepository = questionRepository;
            _quizGroupRepository = quizGroupRepository;
            _quizSectionRepository = quizSectionRepository;
            _mapper = mapper;
            _logger = logger;
            _minioFileStorage = minioFileStorage;
        }

        public async Task<ServiceResponse<QuestionReadDto>> GetQuestionByIdAsync(int questionId)
        {
            var response = new ServiceResponse<QuestionReadDto>();

            try
            {
                var question = await _questionRepository.GetQuestionByIdAsync(questionId);
                if (question == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy câu hỏi.";
                    response.StatusCode = 404;
                    return response;
                }

                var questionDto = _mapper.Map<QuestionReadDto>(question);
                
                // Generate URL cho Question MediaUrl
                if (!string.IsNullOrWhiteSpace(questionDto.MediaUrl))
                {
                    questionDto.MediaUrl = BuildPublicUrl.BuildURL(QuestionBucket, questionDto.MediaUrl);
                }
                
                // Generate URLs cho AnswerOption MediaUrl
                foreach (var option in questionDto.Options)
                {
                    if (!string.IsNullOrWhiteSpace(option.MediaUrl))
                    {
                        option.MediaUrl = BuildPublicUrl.BuildURL(QuestionBucket, option.MediaUrl);
                    }
                }
                
                response.Data = questionDto;
                response.Success = true;
                response.Message = "Lấy thông tin câu hỏi thành công.";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy câu hỏi với ID: {QuestionId}", questionId);
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi lấy thông tin câu hỏi: {ex.Message}";
                response.StatusCode = 500;
            }

            return response;
        }

        public async Task<ServiceResponse<List<QuestionReadDto>>> GetQuestionsByQuizGroupIdAsync(int quizGroupId)
        {
            var response = new ServiceResponse<List<QuestionReadDto>>();

            try
            {
                var questions = await _questionRepository.GetQuestionsByQuizGroupIdAsync(quizGroupId);
                var questionDtos = _mapper.Map<List<QuestionReadDto>>(questions);
                
                // Generate URLs cho tất cả questions và options
                foreach (var questionDto in questionDtos)
                {
                    if (!string.IsNullOrWhiteSpace(questionDto.MediaUrl))
                    {
                        questionDto.MediaUrl = BuildPublicUrl.BuildURL(QuestionBucket, questionDto.MediaUrl);
                    }
                    
                    foreach (var option in questionDto.Options)
                    {
                        if (!string.IsNullOrWhiteSpace(option.MediaUrl))
                        {
                            option.MediaUrl = BuildPublicUrl.BuildURL(QuestionBucket, option.MediaUrl);
                        }
                    }
                }
                
                response.Data = questionDtos;
                response.Success = true;
                response.Message = $"Lấy danh sách {questions.Count} câu hỏi thành công.";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách câu hỏi theo QuizGroupId: {QuizGroupId}", quizGroupId);
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi lấy danh sách câu hỏi: {ex.Message}";
                response.StatusCode = 500;
            }

            return response;
        }

        public async Task<ServiceResponse<List<QuestionReadDto>>> GetQuestionsByQuizSectionIdAsync(int quizSectionId)
        {
            var response = new ServiceResponse<List<QuestionReadDto>>();

            try
            {
                var questions = await _questionRepository.GetQuestionsByQuizSectionIdAsync(quizSectionId);
                var questionDtos = _mapper.Map<List<QuestionReadDto>>(questions);
                
                // Generate URLs cho tất cả questions và options
                foreach (var questionDto in questionDtos)
                {
                    if (!string.IsNullOrWhiteSpace(questionDto.MediaUrl))
                    {
                        questionDto.MediaUrl = BuildPublicUrl.BuildURL(QuestionBucket, questionDto.MediaUrl);
                    }
                    
                    foreach (var option in questionDto.Options)
                    {
                        if (!string.IsNullOrWhiteSpace(option.MediaUrl))
                        {
                            option.MediaUrl = BuildPublicUrl.BuildURL(QuestionBucket, option.MediaUrl);
                        }
                    }
                }
                
                response.Data = questionDtos;
                response.Success = true;
                response.Message = $"Lấy danh sách {questions.Count} câu hỏi thành công.";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách câu hỏi theo QuizSectionId: {QuizSectionId}", quizSectionId);
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi lấy danh sách câu hỏi: {ex.Message}";
                response.StatusCode = 500;
            }

            return response;
        }

        public async Task<ServiceResponse<QuestionReadDto>> AddQuestionAsync(QuestionCreateDto questionCreateDto)
        {
            var response = new ServiceResponse<QuestionReadDto>();

            try
            {
                // Validate QuizGroup exists
                var quizGroup = await _quizGroupRepository.GetQuizGroupByIdAsync(questionCreateDto.QuizGroupId);
                if (quizGroup == null)
                {
                    response.Success = false;
                    response.Message = "Quiz group không tồn tại.";
                    response.StatusCode = 404;
                    return response;
                }

                // Validate QuizSection exists
                var quizSection = await _quizSectionRepository.GetQuizSectionByIdAsync(questionCreateDto.QuizSectionId);
                if (quizSection == null)
                {
                    response.Success = false;
                    response.Message = "Quiz section không tồn tại.";
                    response.StatusCode = 404;
                    return response;
                }

                // Map DTO to Entity
                var question = _mapper.Map<Question>(questionCreateDto);
                question.CreatedAt = DateTime.UtcNow;
                question.UpdatedAt = DateTime.UtcNow;
                
                string? committedQuestionMediaKey = null;
                var committedOptionMediaKeys = new List<(int index, string key)>();
                
                // Commit Question MediaTempKey nếu có
                if (!string.IsNullOrWhiteSpace(questionCreateDto.MediaTempKey))
                {
                    var mediaResult = await _minioFileStorage.CommitFileAsync(
                        questionCreateDto.MediaTempKey,
                        QuestionBucket,
                        QuestionFolder
                    );
                    
                    if (!mediaResult.Success || string.IsNullOrWhiteSpace(mediaResult.Data))
                    {
                        _logger.LogError("Failed to commit question media: {Error}", mediaResult.Message);
                        response.Success = false;
                        response.Message = $"Không thể lưu media câu hỏi: {mediaResult.Message}";
                        response.StatusCode = 400;
                        return response;
                    }
                    
                    committedQuestionMediaKey = mediaResult.Data;
                    question.MediaUrl = committedQuestionMediaKey;
                }
                
                // Commit AnswerOption MediaTempKey nếu có
                for (int i = 0; i < questionCreateDto.Options.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(questionCreateDto.Options[i].MediaTempKey))
                    {
                        var optionResult = await _minioFileStorage.CommitFileAsync(
                            questionCreateDto.Options[i].MediaTempKey,
                            QuestionBucket,
                            QuestionFolder
                        );
                        
                        if (!optionResult.Success || string.IsNullOrWhiteSpace(optionResult.Data))
                        {
                            _logger.LogError("Failed to commit option {Index} media: {Error}", i, optionResult.Message);
                            
                            // Rollback question media
                            if (committedQuestionMediaKey != null)
                            {
                                await _minioFileStorage.DeleteFileAsync(committedQuestionMediaKey, QuestionBucket);
                            }
                            
                            // Rollback already committed option media
                            foreach (var (_, key) in committedOptionMediaKeys)
                            {
                                await _minioFileStorage.DeleteFileAsync(key, QuestionBucket);
                            }
                            
                            response.Success = false;
                            response.Message = $"Không thể lưu media đáp án {i + 1}: {optionResult.Message}";
                            response.StatusCode = 400;
                            return response;
                        }
                        
                        committedOptionMediaKeys.Add((i, optionResult.Data));
                        question.Options[i].MediaUrl = optionResult.Data;
                    }
                }
                
                // Save to database with rollback
                Question createdQuestion = question;
                try
                {
                    await _questionRepository.AddQuestionAsync(question);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while creating question");
                    
                    // Rollback all MinIO files
                    if (committedQuestionMediaKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(committedQuestionMediaKey, QuestionBucket);
                    }
                    foreach (var (_, key) in committedOptionMediaKeys)
                    {
                        await _minioFileStorage.DeleteFileAsync(key, QuestionBucket);
                    }
                    
                    response.Success = false;
                    response.Message = "Lỗi database khi tạo câu hỏi";
                    response.StatusCode = 500;
                    return response;
                }
                
                var questionDto = _mapper.Map<QuestionReadDto>(createdQuestion);
                
                // Generate URLs cho response
                if (!string.IsNullOrWhiteSpace(questionDto.MediaUrl))
                {
                    questionDto.MediaUrl = BuildPublicUrl.BuildURL(QuestionBucket, questionDto.MediaUrl);
                }
                foreach (var option in questionDto.Options)
                {
                    if (!string.IsNullOrWhiteSpace(option.MediaUrl))
                    {
                        option.MediaUrl = BuildPublicUrl.BuildURL(QuestionBucket, option.MediaUrl);
                    }
                }
                
                response.Data = questionDto;
                response.Success = true;
                response.Message = "Tạo câu hỏi thành công.";
                response.StatusCode = 201;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo câu hỏi");
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi tạo câu hỏi: {ex.Message}";
                response.StatusCode = 500;
            }

            return response;
        }

        public async Task<ServiceResponse<QuestionReadDto>> UpdateQuestionAsync(int questionId, QuestionUpdateDto questionUpdateDto)
        {
            var response = new ServiceResponse<QuestionReadDto>();

            try
            {
                var existingQuestion = await _questionRepository.GetQuestionByIdAsync(questionId);
                if (existingQuestion == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy câu hỏi.";
                    response.StatusCode = 404;
                    return response;
                }

                // Validate QuizGroup exists if changed
                if (questionUpdateDto.QuizGroupId != existingQuestion.QuizGroupId)
                {
                    var quizGroup = await _quizGroupRepository.GetQuizGroupByIdAsync(questionUpdateDto.QuizGroupId);
                    if (quizGroup == null)
                    {
                        response.Success = false;
                        response.Message = "Quiz group không tồn tại.";
                        response.StatusCode = 404;
                        return response;
                    }
                }

                // Validate QuizSection exists if changed
                if (questionUpdateDto.QuizSectionId != existingQuestion.QuizSectionId)
                {
                    var quizSection = await _quizSectionRepository.GetQuizSectionByIdAsync(questionUpdateDto.QuizSectionId);
                    if (quizSection == null)
                    {
                        response.Success = false;
                        response.Message = "Quiz section không tồn tại.";
                        response.StatusCode = 404;
                        return response;
                    }
                }

                // Map changes from DTO to entity
                _mapper.Map(questionUpdateDto, existingQuestion);
                existingQuestion.UpdatedAt = DateTime.UtcNow;
                
                string? newQuestionMediaKey = null;
                string? oldQuestionMediaKey = !string.IsNullOrWhiteSpace(existingQuestion.MediaUrl) 
                    ? existingQuestion.MediaUrl 
                    : null;
                var newOptionMediaKeys = new List<(int index, string key)>();
                var oldOptionMediaKeys = new List<(int index, string key)>();
                
                // Commit Question MediaUrl mới nếu có
                if (!string.IsNullOrWhiteSpace(questionUpdateDto.MediaTempKey))
                {
                    var questionMediaResult = await _minioFileStorage.CommitFileAsync(
                        questionUpdateDto.MediaTempKey,
                        QuestionBucket,
                        QuestionFolder
                    );
                    
                    if (!questionMediaResult.Success || string.IsNullOrWhiteSpace(questionMediaResult.Data))
                    {
                        _logger.LogError("Failed to commit question media: {Error}", questionMediaResult.Message);
                        response.Success = false;
                        response.Message = $"Không thể lưu media câu hỏi: {questionMediaResult.Message}";
                        response.StatusCode = 400;
                        return response;
                    }
                    
                    newQuestionMediaKey = questionMediaResult.Data;
                    existingQuestion.MediaUrl = newQuestionMediaKey;
                }
                
                // Commit AnswerOption MediaUrls mới
                for (int i = 0; i < questionUpdateDto.Options.Count && i < existingQuestion.Options.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(questionUpdateDto.Options[i].MediaTempKey))
                    {
                        // Track old media
                        if (!string.IsNullOrWhiteSpace(existingQuestion.Options[i].MediaUrl))
                        {
                            oldOptionMediaKeys.Add((i, existingQuestion.Options[i].MediaUrl));
                        }
                        
                        // Commit new media
                        var optionResult = await _minioFileStorage.CommitFileAsync(
                            questionUpdateDto.Options[i].MediaTempKey,
                            QuestionBucket,
                            QuestionFolder
                        );
                        
                        if (!optionResult.Success || string.IsNullOrWhiteSpace(optionResult.Data))
                        {
                            _logger.LogError("Failed to commit option {Index} media: {Error}", i, optionResult.Message);
                            
                            // Rollback question media if committed
                            if (newQuestionMediaKey != null)
                            {
                                await _minioFileStorage.DeleteFileAsync(newQuestionMediaKey, QuestionBucket);
                            }
                            
                            // Rollback already committed option media
                            foreach (var (_, key) in newOptionMediaKeys)
                            {
                                await _minioFileStorage.DeleteFileAsync(key, QuestionBucket);
                            }
                            
                            response.Success = false;
                            response.Message = $"Không thể lưu media đáp án {i + 1}: {optionResult.Message}";
                            response.StatusCode = 400;
                            return response;
                        }
                        
                        newOptionMediaKeys.Add((i, optionResult.Data));
                        existingQuestion.Options[i].MediaUrl = optionResult.Data;
                    }
                }
                
                // Save to database with rollback
                try
                {
                    await _questionRepository.UpdateQuestionAsync(existingQuestion);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while updating question");
                    
                    // Rollback all new MinIO files
                    if (newQuestionMediaKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(newQuestionMediaKey, QuestionBucket);
                    }
                    foreach (var (_, key) in newOptionMediaKeys)
                    {
                        await _minioFileStorage.DeleteFileAsync(key, QuestionBucket);
                    }
                    
                    response.Success = false;
                    response.Message = "Lỗi database khi cập nhật câu hỏi";
                    response.StatusCode = 500;
                    return response;
                }
                
                // Delete old files only after successful DB update
                if (oldQuestionMediaKey != null)
                {
                    await _minioFileStorage.DeleteFileAsync(oldQuestionMediaKey, QuestionBucket);
                }
                foreach (var (_, key) in oldOptionMediaKeys)
                {
                    await _minioFileStorage.DeleteFileAsync(key, QuestionBucket);
                }

                var questionDto = _mapper.Map<QuestionReadDto>(existingQuestion);
                
                // Generate URLs cho response
                if (!string.IsNullOrWhiteSpace(questionDto.MediaUrl))
                {
                    questionDto.MediaUrl = BuildPublicUrl.BuildURL(QuestionBucket, questionDto.MediaUrl);
                }
                foreach (var option in questionDto.Options)
                {
                    if (!string.IsNullOrWhiteSpace(option.MediaUrl))
                    {
                        option.MediaUrl = BuildPublicUrl.BuildURL(QuestionBucket, option.MediaUrl);
                    }
                }
                
                response.Data = questionDto;
                response.Success = true;
                response.Message = "Cập nhật câu hỏi thành công.";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật câu hỏi với ID: {QuestionId}", questionId);
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi cập nhật câu hỏi: {ex.Message}";
                response.StatusCode = 500;
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteQuestionAsync(int questionId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var question = await _questionRepository.GetQuestionByIdAsync(questionId);
                if (question == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy câu hỏi.";
                    response.StatusCode = 404;
                    return response;
                }
                
                // Xóa Question MediaUrl từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(question.MediaUrl))
                {
                    await _minioFileStorage.DeleteFileAsync(QuestionBucket, question.MediaUrl);
                }
                
                // Xóa tất cả AnswerOption MediaUrls từ MinIO
                foreach (var option in question.Options)
                {
                    if (!string.IsNullOrWhiteSpace(option.MediaUrl))
                    {
                        await _minioFileStorage.DeleteFileAsync(QuestionBucket, option.MediaUrl);
                    }
                }

                await _questionRepository.DeleteQuestionAsync(questionId);

                response.Data = true;
                response.Success = true;
                response.Message = "Xóa câu hỏi thành công.";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa câu hỏi với ID: {QuestionId}", questionId);
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi xóa câu hỏi: {ex.Message}";
                response.StatusCode = 500;
            }

            return response;
        }

        public async Task<ServiceResponse<QuestionBulkResponseDto>> AddBulkQuestionsAsync(QuestionBulkCreateDto questionBulkCreateDto)
        {
            var response = new ServiceResponse<QuestionBulkResponseDto>();

            try
            {
                if (questionBulkCreateDto.Questions == null || !questionBulkCreateDto.Questions.Any())
                {
                    response.Success = false;
                    response.Message = "Danh sách câu hỏi không được để trống.";
                    response.StatusCode = 400;
                    return response;
                }

                // Validate all QuizGroup and QuizSection IDs exist
                var quizGroupIds = questionBulkCreateDto.Questions.Select(q => q.QuizGroupId).Distinct().ToList();
                var quizSectionIds = questionBulkCreateDto.Questions.Select(q => q.QuizSectionId).Distinct().ToList();

                foreach (var groupId in quizGroupIds)
                {
                    var quizGroup = await _quizGroupRepository.GetQuizGroupByIdAsync(groupId);
                    if (quizGroup == null)
                    {
                        response.Success = false;
                        response.Message = $"Quiz group với ID {groupId} không tồn tại.";
                        response.StatusCode = 404;
                        return response;
                    }
                }

                foreach (var sectionId in quizSectionIds)
                {
                    var quizSection = await _quizSectionRepository.GetQuizSectionByIdAsync(sectionId);
                    if (quizSection == null)
                    {
                        response.Success = false;
                        response.Message = $"Quiz section với ID {sectionId} không tồn tại.";
                        response.StatusCode = 404;
                        return response;
                    }
                }

                // Map all DTOs to entities
                var questions = new List<Question>();
                foreach (var questionDto in questionBulkCreateDto.Questions)
                {
                    var question = _mapper.Map<Question>(questionDto);
                    question.CreatedAt = DateTime.UtcNow;
                    question.UpdatedAt = DateTime.UtcNow;

                    // Map answer options - EF Core sẽ tự động insert cả Options
                    // vì Options đã được thêm vào question.Options collection qua AutoMapper
                    questions.Add(question);
                }

                // Bulk insert với transaction
                var createdQuestionIds = await _questionRepository.AddBulkQuestionsWithTransactionAsync(questions);

                response.Data = new QuestionBulkResponseDto
                {
                    CreatedQuestionIds = createdQuestionIds
                };
                response.Success = true;
                response.Message = $"Tạo thành công {createdQuestionIds.Count} câu hỏi với tất cả đáp án.";
                response.StatusCode = 201;

                _logger.LogInformation("Bulk created {Count} questions with their answer options", createdQuestionIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hàng loạt câu hỏi");
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi tạo hàng loạt câu hỏi: {ex.Message}";
                response.StatusCode = 500;
            }

            return response;
        }
    }
}
