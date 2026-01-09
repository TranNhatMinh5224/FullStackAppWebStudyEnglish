using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
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
        private readonly IQuizRepository _quizRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestionService> _logger;
        private readonly IQuestionMediaService _questionMediaService;

        public QuestionService(
            IQuestionRepository questionRepository,
            IQuizGroupRepository quizGroupRepository,
            IQuizSectionRepository quizSectionRepository,
            IQuizRepository quizRepository,
            IMapper mapper,
            ILogger<QuestionService> logger,
            IQuestionMediaService questionMediaService)
        {
            _questionRepository = questionRepository;
            _quizGroupRepository = quizGroupRepository;
            _quizSectionRepository = quizSectionRepository;
            _quizRepository = quizRepository;
            _mapper = mapper;
            _logger = logger;
            _questionMediaService = questionMediaService;
        }

        /// <summary>
        /// Helper method to build media URLs for question and its options
        /// </summary>
        private void BuildQuestionMediaUrls(QuestionReadDto questionDto)
        {
            if (!string.IsNullOrWhiteSpace(questionDto.MediaUrl))
            {
                questionDto.MediaUrl = _questionMediaService.BuildMediaUrl(questionDto.MediaUrl);
            }

            foreach (var option in questionDto.Options)
            {
                if (!string.IsNullOrWhiteSpace(option.MediaUrl))
                {
                    option.MediaUrl = _questionMediaService.BuildMediaUrl(option.MediaUrl);
                }
            }
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

                // Generate URLs using helper
                BuildQuestionMediaUrls(questionDto);

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

                // Generate URLs using helper
                foreach (var questionDto in questionDtos)
                {
                    BuildQuestionMediaUrls(questionDto);
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

                // Generate URLs using helper
                foreach (var questionDto in questionDtos)
                {
                    BuildQuestionMediaUrls(questionDto);
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
                // Validate QuizGroup exists if provided
                if (questionCreateDto.QuizGroupId.HasValue)
                {
                    var quizGroup = await _quizGroupRepository.GetQuizGroupByIdAsync(questionCreateDto.QuizGroupId.Value);
                    if (quizGroup == null)
                    {
                        response.Success = false;
                        response.Message = "Quiz group không tồn tại.";
                        response.StatusCode = 404;
                        return response;
                    }
                }

                // Validate QuizSection exists if provided
                if (questionCreateDto.QuizSectionId.HasValue)
                {
                    var quizSection = await _quizSectionRepository.GetQuizSectionByIdAsync(questionCreateDto.QuizSectionId.Value);
                    if (quizSection == null)
                    {
                        response.Success = false;
                        response.Message = "Quiz section không tồn tại.";
                        response.StatusCode = 404;
                        return response;
                    }
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
                    try
                    {
                        committedQuestionMediaKey = await _questionMediaService.CommitMediaAsync(questionCreateDto.MediaTempKey);
                        question.MediaKey = committedQuestionMediaKey;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to commit question media");
                        response.Success = false;
                        response.Message = "Không thể lưu media câu hỏi";
                        response.StatusCode = 400;
                        return response;
                    }
                }

                // Commit AnswerOption MediaTempKey nếu có
                for (int i = 0; i < questionCreateDto.Options.Count; i++)
                {
                    var mediaTempKey = questionCreateDto.Options[i].MediaTempKey;
                    if (!string.IsNullOrWhiteSpace(mediaTempKey))
                    {
                        try
                        {
                            var optionMediaKey = await _questionMediaService.CommitMediaAsync(mediaTempKey);
                            committedOptionMediaKeys.Add((i, optionMediaKey));
                            question.Options[i].MediaKey = optionMediaKey;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to commit option {Index} media", i);

                            // Rollback question media
                            if (committedQuestionMediaKey != null)
                            {
                                await _questionMediaService.DeleteMediaAsync(committedQuestionMediaKey);
                            }

                            // Rollback already committed option media
                            foreach (var (_, key) in committedOptionMediaKeys)
                            {
                                await _questionMediaService.DeleteMediaAsync(key);
                            }

                            response.Success = false;
                            response.Message = $"Không thể lưu media đáp án {i + 1}";
                            response.StatusCode = 400;
                            return response;
                        }
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
                        await _questionMediaService.DeleteMediaAsync(committedQuestionMediaKey);
                    }

                    foreach (var (_, key) in committedOptionMediaKeys)
                    {
                        await _questionMediaService.DeleteMediaAsync(key);
                    }

                    response.Success = false;
                    response.Message = "Lỗi database khi tạo câu hỏi";
                    response.StatusCode = 500;
                    return response;
                }

                var questionDto = _mapper.Map<QuestionReadDto>(createdQuestion);

                // Generate URLs using helper
                BuildQuestionMediaUrls(questionDto);

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

                // Validate QuizGroup exists if changed and provided
                if (questionUpdateDto.QuizGroupId.HasValue && questionUpdateDto.QuizGroupId != existingQuestion.QuizGroupId)
                {
                    var quizGroup = await _quizGroupRepository.GetQuizGroupByIdAsync(questionUpdateDto.QuizGroupId.Value);
                    if (quizGroup == null)
                    {
                        response.Success = false;
                        response.Message = "Quiz group không tồn tại.";
                        response.StatusCode = 404;
                        return response;
                    }
                }

                // Validate QuizSection exists if changed and provided
                if (questionUpdateDto.QuizSectionId.HasValue && questionUpdateDto.QuizSectionId != existingQuestion.QuizSectionId)
                {
                    var quizSection = await _quizSectionRepository.GetQuizSectionByIdAsync(questionUpdateDto.QuizSectionId.Value);
                    if (quizSection == null)
                    {
                        response.Success = false;
                        response.Message = "Quiz section không tồn tại.";
                        response.StatusCode = 404;
                        return response;
                    }
                }

                // Backup media keys cũ trước khi map
                string? oldQuestionMediaKey = !string.IsNullOrWhiteSpace(existingQuestion.MediaKey)
                    ? existingQuestion.MediaKey
                    : null;

                var optionOldMediaKeys = existingQuestion.Options
                    .Select(o => o.MediaKey)
                    .ToList();

                // Map changes from DTO to entity (không làm mất backup)
                _mapper.Map(questionUpdateDto, existingQuestion);
                existingQuestion.UpdatedAt = DateTime.UtcNow;

                string? newQuestionMediaKey = null;
                var newOptionMediaKeys = new List<(int index, string key)>();
                var oldOptionMediaKeys = new List<(int index, string key)>();

                // Commit Question Media mới nếu có
                if (!string.IsNullOrWhiteSpace(questionUpdateDto.MediaTempKey))
                {
                    try
                    {
                        newQuestionMediaKey = await _questionMediaService.CommitMediaAsync(questionUpdateDto.MediaTempKey);
                        existingQuestion.MediaKey = newQuestionMediaKey;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to commit question media");
                        response.Success = false;
                        response.Message = "Không thể lưu media câu hỏi";
                        response.StatusCode = 400;
                        return response;
                    }
                }

                // Commit AnswerOption Media mới
                for (int i = 0; i < questionUpdateDto.Options.Count && i < existingQuestion.Options.Count; i++)
                {
                    var tempKey = questionUpdateDto.Options[i].MediaTempKey;
                    if (!string.IsNullOrWhiteSpace(tempKey))
                    {
                        // Backup old media key của option này trước khi thay thế
                        string? oldMediaKey = (optionOldMediaKeys.Count > i && !string.IsNullOrWhiteSpace(optionOldMediaKeys[i]))
                            ? optionOldMediaKeys[i]
                            : null;

                        if (oldMediaKey != null)
                        {
                            oldOptionMediaKeys.Add((i, oldMediaKey));
                        }

                        try
                        {
                            var optionMediaKey = await _questionMediaService.CommitMediaAsync(tempKey);
                            newOptionMediaKeys.Add((i, optionMediaKey));
                            existingQuestion.Options[i].MediaKey = optionMediaKey;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to commit option {Index} media", i);

                            // Rollback question media nếu đã commit
                            if (newQuestionMediaKey != null)
                            {
                                await _questionMediaService.DeleteMediaAsync(newQuestionMediaKey);
                            }

                            // Rollback các option media mới đã commit
                            foreach (var (_, key) in newOptionMediaKeys)
                            {
                                await _questionMediaService.DeleteMediaAsync(key);
                            }

                            response.Success = false;
                            response.Message = $"Không thể lưu media đáp án {i + 1}";
                            response.StatusCode = 400;
                            return response;
                        }
                    }
                }

                // Save to database với rollback
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
                        await _questionMediaService.DeleteMediaAsync(newQuestionMediaKey);
                    }

                    foreach (var (_, key) in newOptionMediaKeys)
                    {
                        await _questionMediaService.DeleteMediaAsync(key);
                    }

                    response.Success = false;
                    response.Message = "Lỗi database khi cập nhật câu hỏi";
                    response.StatusCode = 500;
                    return response;
                }

                // Delete old files chỉ sau khi DB update thành công
                if (oldQuestionMediaKey != null && newQuestionMediaKey != null)
                {
                    await _questionMediaService.DeleteMediaAsync(oldQuestionMediaKey);
                }

                foreach (var (_, key) in oldOptionMediaKeys)
                {
                    await _questionMediaService.DeleteMediaAsync(key);
                }

                var questionDto = _mapper.Map<QuestionReadDto>(existingQuestion);

                // Generate URLs using helper
                BuildQuestionMediaUrls(questionDto);

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

                // Xóa Question Media từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(question.MediaKey))
                {
                    await _questionMediaService.DeleteMediaAsync(question.MediaKey);
                }

                // Xóa tất cả AnswerOption Media từ MinIO
                foreach (var option in question.Options)
                {
                    if (!string.IsNullOrWhiteSpace(option.MediaKey))
                    {
                        await _questionMediaService.DeleteMediaAsync(option.MediaKey);
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
                if (questionBulkCreateDto.Questions == null || questionBulkCreateDto.Questions.Count == 0)
                {
                    response.Success = false;
                    response.Message = "Danh sách câu hỏi không được để trống.";
                    response.StatusCode = 400;
                    return response;
                }

                // Validate all QuizGroup and QuizSection IDs exist
                var quizGroupIds = questionBulkCreateDto.Questions
                    .Select(q => q.QuizGroupId)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();

                var quizSectionIds = questionBulkCreateDto.Questions
                    .Select(q => q.QuizSectionId)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();

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

        public async Task<ServiceResponse<QuizSectionDto>> CreateQuizSectionBulkAsync(QuizSectionBulkCreateDto sectionBulkCreateDto)
        {
            var response = new ServiceResponse<QuizSectionDto>();

            try
            {
                // 1. Validate Quiz exists
                var quiz = await _quizRepository.GetQuizByIdAsync(sectionBulkCreateDto.QuizId);
                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = $"Quiz với ID {sectionBulkCreateDto.QuizId} không tồn tại.";
                    response.StatusCode = 404;
                    return response;
                }

                // 2. Validate total question count
                var questionsInGroups = sectionBulkCreateDto.QuizGroups.Sum(g => g.Questions?.Count ?? 0);
                var standaloneCount = sectionBulkCreateDto.StandaloneQuestions?.Count ?? 0;
                var totalQuestions = questionsInGroups + standaloneCount;

                if (totalQuestions == 0)
                {
                    response.Success = false;
                    response.Message = "Section phải có ít nhất 1 câu hỏi.";
                    response.StatusCode = 400;
                    return response;
                }

                _logger.LogInformation("Creating bulk section with {GroupCount} groups, {GroupQuestions} questions in groups, {StandaloneCount} standalone questions",
                    sectionBulkCreateDto.QuizGroups.Count, questionsInGroups, standaloneCount);

                // 3. Create QuizSection
                var quizSection = new QuizSection
                {
                    QuizId = sectionBulkCreateDto.QuizId,
                    Title = sectionBulkCreateDto.Title,
                    Description = sectionBulkCreateDto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _quizSectionRepository.AddQuizSectionAsync(quizSection);
                await _quizSectionRepository.SaveChangesAsync();

                _logger.LogInformation("Created QuizSection with ID: {SectionId}", quizSection.QuizSectionId);

                var createdGroupIds = new List<int>();
                var createdQuestionIds = new List<int>();

                // 4. Create each QuizGroup and its Questions
                foreach (var groupDto in sectionBulkCreateDto.QuizGroups)
                {
                    // 4.1. Create QuizGroup using AutoMapper
                    var quizGroup = _mapper.Map<QuizGroup>(groupDto);
                    quizGroup.QuizSectionId = quizSection.QuizSectionId;
                    quizGroup.CreatedAt = DateTime.UtcNow;
                    quizGroup.UpdatedAt = DateTime.UtcNow;

                    await _quizGroupRepository.AddQuizGroupAsync(quizGroup);
                    await _quizGroupRepository.SaveChangesAsync();

                    createdGroupIds.Add(quizGroup.QuizGroupId);
                    _logger.LogInformation("Created QuizGroup with ID: {GroupId}", quizGroup.QuizGroupId);

                    // 4.2. Create Questions for this group
                    foreach (var questionDto in groupDto.Questions)
                    {
                        // Map using AutoMapper
                        var question = _mapper.Map<Question>(questionDto);
                        question.QuizSectionId = quizSection.QuizSectionId;
                        question.QuizGroupId = quizGroup.QuizGroupId;
                        question.CreatedAt = DateTime.UtcNow;
                        question.UpdatedAt = DateTime.UtcNow;

                        await _questionRepository.AddQuestionAsync(question);
                        await _questionRepository.SaveChangesAsync();

                        createdQuestionIds.Add(question.QuestionId);

                        // 4.3. Create AnswerOptions if needed
                        if (questionDto.Options != null && questionDto.Options.Count > 0)
                        {
                            foreach (var optionDto in questionDto.Options)
                            {
                                var answerOption = _mapper.Map<AnswerOption>(optionDto);
                                answerOption.QuestionId = question.QuestionId;

                                await _questionRepository.AddAnswerOptionAsync(answerOption);
                            }

                            await _questionRepository.SaveChangesAsync();
                        }
                    }
                }

                // 5. Create standalone questions (không thuộc group)
                if (sectionBulkCreateDto.StandaloneQuestions != null && sectionBulkCreateDto.StandaloneQuestions.Count > 0)
                {
                    _logger.LogInformation("Creating {Count} standalone questions for section", sectionBulkCreateDto.StandaloneQuestions.Count);

                    foreach (var questionDto in sectionBulkCreateDto.StandaloneQuestions)
                    {
                        var question = _mapper.Map<Question>(questionDto);
                        question.QuizSectionId = quizSection.QuizSectionId;
                        question.QuizGroupId = null; // Standalone không thuộc group
                        question.CreatedAt = DateTime.UtcNow;
                        question.UpdatedAt = DateTime.UtcNow;

                        await _questionRepository.AddQuestionAsync(question);
                        await _questionRepository.SaveChangesAsync();

                        createdQuestionIds.Add(question.QuestionId);

                        // Create AnswerOptions
                        if (questionDto.Options != null && questionDto.Options.Count > 0)
                        {
                            foreach (var optionDto in questionDto.Options)
                            {
                                var answerOption = _mapper.Map<AnswerOption>(optionDto);
                                answerOption.QuestionId = question.QuestionId;

                                await _questionRepository.AddAnswerOptionAsync(answerOption);
                            }

                            await _questionRepository.SaveChangesAsync();
                        }
                    }
                }

                // 6. Return created section
                var createdSection = await _quizSectionRepository.GetQuizSectionByIdAsync(quizSection.QuizSectionId);
                var sectionDto = _mapper.Map<QuizSectionDto>(createdSection);

                response.Data = sectionDto;
                response.Success = true;
                response.Message = $"Tạo thành công section với {createdGroupIds.Count} groups và {createdQuestionIds.Count} câu hỏi (trong đó {questionsInGroups} trong groups, {standaloneCount} standalone).";
                response.StatusCode = 201;

                _logger.LogInformation(
                    "Successfully created bulk section: SectionId={SectionId}, Groups={GroupCount}, TotalQuestions={QuestionCount}, InGroups={InGroups}, Standalone={Standalone}",
                    quizSection.QuizSectionId, createdGroupIds.Count, createdQuestionIds.Count, questionsInGroups, standaloneCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bulk quiz section");
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi tạo section: {ex.Message}";
                response.StatusCode = 500;
            }

            return response;
        }
    }
}
