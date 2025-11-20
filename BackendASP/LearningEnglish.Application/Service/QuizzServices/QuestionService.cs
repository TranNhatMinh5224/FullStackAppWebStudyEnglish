using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
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

        public QuestionService(
            IQuestionRepository questionRepository,
            IQuizGroupRepository quizGroupRepository,
            IQuizSectionRepository quizSectionRepository,
            IMapper mapper,
            ILogger<QuestionService> logger)
        {
            _questionRepository = questionRepository;
            _quizGroupRepository = quizGroupRepository;
            _quizSectionRepository = quizSectionRepository;
            _mapper = mapper;
            _logger = logger;
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

                response.Data = _mapper.Map<QuestionReadDto>(question);
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
                response.Data = _mapper.Map<List<QuestionReadDto>>(questions);
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
                response.Data = _mapper.Map<List<QuestionReadDto>>(questions);
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

                // Save to database
                await _questionRepository.AddQuestionAsync(question);

                response.Data = _mapper.Map<QuestionReadDto>(question);
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

                await _questionRepository.UpdateQuestionAsync(existingQuestion);

                response.Data = _mapper.Map<QuestionReadDto>(existingQuestion);
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
