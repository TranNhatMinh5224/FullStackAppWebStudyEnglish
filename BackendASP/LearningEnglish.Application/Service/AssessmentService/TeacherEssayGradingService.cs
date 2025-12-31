using AutoMapper;
using LearningEnglish.Application.Common.Prompts;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.EssayGrading;


public class TeacherEssayGradingService : ITeacherEssayGradingService
{
    private readonly IEssaySubmissionRepository _submissionRepository;
    private readonly IEssayRepository _essayRepository;
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IGeminiService _geminiService;
    private readonly IAiResponseParser _responseParser;
    private readonly IMapper _mapper;
    private readonly ILogger<TeacherEssayGradingService> _logger;

    public TeacherEssayGradingService(
        IEssaySubmissionRepository submissionRepository,
        IEssayRepository essayRepository,
        IAssessmentRepository assessmentRepository,
        IGeminiService geminiService,
        IAiResponseParser responseParser,
        IMapper mapper,
        ILogger<TeacherEssayGradingService> logger)
    {
        _submissionRepository = submissionRepository;
        _essayRepository = essayRepository;
        _assessmentRepository = assessmentRepository;
        _geminiService = geminiService;
        _responseParser = responseParser;
        _mapper = mapper;
        _logger = logger;
    }

    private async Task<bool> ValidateSubmissionOwnershipAsync(int submissionId, int teacherId)
    {
        var submission = await _submissionRepository.GetSubmissionByIdAsync(submissionId);
        if (submission?.Essay == null)
            return false;

        var essay = await _essayRepository.GetEssayByIdAsync(submission.EssayId);
        if (essay == null)
            return false;

        // Check if teacher owns the essay through assessment's module's lesson's course
        return essay.Assessment?.Module?.Lesson?.Course?.TeacherId == teacherId;
    }

    public async Task<ServiceResponse<EssayGradingResultDto>> GradeEssayWithAIAsync(
        int submissionId,
        int teacherId,
        CancellationToken cancellationToken = default)
    {
        var response = new ServiceResponse<EssayGradingResultDto>();
        
        try
        {
            _logger.LogInformation("📝 Teacher {TeacherId} starting AI grading for submission {SubmissionId}", teacherId, submissionId);

            if (!await ValidateSubmissionOwnershipAsync(submissionId, teacherId))
            {
                response.Success = false;
                response.StatusCode = 403;
                response.Message = "Bạn không có quyền chấm bài nộp này";
                return response;
            }

            var submission = await _submissionRepository.GetSubmissionByIdAsync(submissionId);
            if (submission == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = $"Không tìm thấy bài nộp với ID {submissionId}";
                return response;
            }

            var essay = await _essayRepository.GetEssayByIdAsync(submission.EssayId);
            if (essay == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "Không tìm thấy đề bài essay";
                return response;
            }

            var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
            if (assessment == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "Không tìm thấy bài kiểm tra";
                return response;
            }

            if (string.IsNullOrWhiteSpace(submission.TextContent))
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "Bài làm chỉ có file đính kèm. AI không thể chấm tự động. Vui lòng chấm thủ công.";
                return response;
            }

            var maxScore = essay.TotalPoints;
            var prompt = EssayGradingPrompt.BuildPrompt(
                essay.Title,
                essay.Description ?? string.Empty,
                submission.TextContent,
                maxScore
            );

            var geminiResponse = await _geminiService.GenerateContentAsync(prompt, cancellationToken);
            if (!geminiResponse.Success)
            {
                return new ServiceResponse<EssayGradingResultDto>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"AI grading failed: {geminiResponse.ErrorMessage}"
                };
            }

            // Use centralized response parser
            var aiResult = _responseParser.ParseGradingResponse(geminiResponse.Content);

            if (aiResult.Score > maxScore)
            {
                _logger.LogWarning("⚠️ AI score {Score} exceeds max score {MaxScore}, adjusting...", aiResult.Score, maxScore);
                aiResult.Score = maxScore;
            }

            submission.Score = aiResult.Score;
            submission.Feedback = aiResult.Feedback;
            submission.GradedAt = DateTime.UtcNow;
            submission.Status = SubmissionStatus.Graded;

            await _submissionRepository.UpdateSubmissionAsync(submission);

            _logger.LogInformation("✅ Teacher {TeacherId} AI grading completed for submission {SubmissionId}. Score: {Score}/{MaxScore}", 
                teacherId, submissionId, aiResult.Score, maxScore);

            var result = _mapper.Map<EssayGradingResultDto>(submission);
            result.MaxScore = maxScore;
            result.Breakdown = aiResult.Breakdown;
            result.Strengths = aiResult.Strengths;
            result.Improvements = aiResult.Improvements;

            response.Success = true;
            response.StatusCode = 200;
            response.Message = "Chấm điểm AI thành công";
            response.Data = result;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error grading submission {SubmissionId}", submissionId);
            response.Success = false;
            response.StatusCode = 500;
            response.Message = "Có lỗi xảy ra khi chấm điểm bài essay";
            return response;
        }
    }

    public async Task<ServiceResponse<EssayGradingResultDto>> GradeEssayAsync(
        int submissionId, 
        TeacherGradingDto dto, 
        int teacherId, 
        CancellationToken cancellationToken = default)
    {
        var response = new ServiceResponse<EssayGradingResultDto>();
        
        try
        {
            _logger.LogInformation("👨‍🏫 Teacher {TeacherId} grading submission {SubmissionId}", teacherId, submissionId);

            if (!await ValidateSubmissionOwnershipAsync(submissionId, teacherId))
            {
                response.Success = false;
                response.StatusCode = 403;
                response.Message = "Bạn không có quyền chấm bài nộp này";
                return response;
            }

            var submission = await _submissionRepository.GetSubmissionByIdAsync(submissionId);
            if (submission == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = $"Không tìm thấy bài nộp với ID {submissionId}";
                return response;
            }

            var essay = await _essayRepository.GetEssayByIdAsync(submission.EssayId);
            if (essay == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "Không tìm thấy đề bài essay";
                return response;
            }

            var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
            if (assessment == null)
            {
                response.Success = false;
                response.StatusCode = 404;
                response.Message = "Không tìm thấy bài kiểm tra";
                return response;
            }

            var maxScore = essay.TotalPoints;

            if (dto.Score > maxScore)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = $"Điểm giáo viên chấm ({dto.Score}) vượt quá điểm tối đa ({maxScore})";
                return response;
            }

            if (dto.Score < 0)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "Điểm không thể âm";
                return response;
            }

            submission.TeacherScore = dto.Score;
            submission.TeacherFeedback = dto.Feedback;
            submission.GradedByTeacherId = teacherId;
            submission.TeacherGradedAt = DateTime.UtcNow;
            submission.Status = SubmissionStatus.Graded;

            await _submissionRepository.UpdateSubmissionAsync(submission);

            _logger.LogInformation("✅ Teacher {TeacherId} grading completed for submission {SubmissionId}. Score: {Score}/{MaxScore}", 
                teacherId, submissionId, dto.Score, maxScore);

            var result = _mapper.Map<EssayGradingResultDto>(submission);
            result.MaxScore = maxScore;

            response.Success = true;
            response.StatusCode = 200;
            response.Message = "Chấm điểm thành công bởi giáo viên";
            response.Data = result;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in teacher grading for submission {SubmissionId}", submissionId);
            response.Success = false;
            response.StatusCode = 500;
            response.Message = "Có lỗi xảy ra khi giáo viên chấm điểm";
            return response;
        }
    }

    public async Task<ServiceResponse<EssayGradingResultDto>> UpdateGradeAsync(
            int submissionId, 
            TeacherGradingDto dto, 
            int teacherId, 
            CancellationToken cancellationToken = default)
        {
            var response = new ServiceResponse<EssayGradingResultDto>();
            
            try
            {
                _logger.LogInformation("👨‍🏫 Teacher {TeacherId} updating grade for submission {SubmissionId}", teacherId, submissionId);

                if (!await ValidateSubmissionOwnershipAsync(submissionId, teacherId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền cập nhật điểm bài nộp này";
                    return response;
                }

                var submission = await _submissionRepository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = $"Không tìm thấy bài nộp với ID {submissionId}";
                    return response;
                }

                // Kiểm tra xem đã có điểm Teacher chưa (chỉ update khi đã chấm rồi)
                if (submission.TeacherScore == null && submission.GradedByTeacherId != teacherId)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Bài nộp này chưa được chấm điểm. Vui lòng sử dụng API chấm điểm thay vì cập nhật.";
                    return response;
                }

                var essay = await _essayRepository.GetEssayByIdAsync(submission.EssayId);
                if (essay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy đề bài essay";
                    return response;
                }

                var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài kiểm tra";
                    return response;
                }

                var maxScore = essay.TotalPoints;

                if (dto.Score > maxScore)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Điểm ({dto.Score}) vượt quá điểm tối đa ({maxScore})";
                    return response;
                }

                if (dto.Score < 0)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Điểm không thể âm";
                    return response;
                }

                // Cập nhật điểm Teacher (giữ nguyên AI score)
                submission.TeacherScore = dto.Score;
                submission.TeacherFeedback = dto.Feedback;
                submission.GradedByTeacherId = teacherId; // Đảm bảo set teacherId
                submission.TeacherGradedAt = DateTime.UtcNow; // Cập nhật thời gian
                submission.Status = SubmissionStatus.Graded;

                await _submissionRepository.UpdateSubmissionAsync(submission);

                _logger.LogInformation("✅ Teacher {TeacherId} updated grade for submission {SubmissionId}. New Score: {Score}/{MaxScore}", 
                    teacherId, submissionId, dto.Score, maxScore);

                var result = _mapper.Map<EssayGradingResultDto>(submission);
                result.MaxScore = maxScore;

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Cập nhật điểm thành công";
                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating grade for submission {SubmissionId}", submissionId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi cập nhật điểm";
                return response;
            }
        }
    }
