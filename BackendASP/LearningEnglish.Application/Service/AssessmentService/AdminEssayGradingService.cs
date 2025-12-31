using AutoMapper;
using LearningEnglish.Application.Common.Prompts;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.EssayGrading;


public class AdminEssayGradingService : IAdminEssayGradingService
{
    private readonly IEssaySubmissionRepository _submissionRepository;
    private readonly IEssayRepository _essayRepository;
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IGeminiService _geminiService;
    private readonly IAiResponseParser _responseParser;
    private readonly IMapper _mapper;
    private readonly ILogger<AdminEssayGradingService> _logger;

    public AdminEssayGradingService(
        IEssaySubmissionRepository submissionRepository,
        IEssayRepository essayRepository,
        IAssessmentRepository assessmentRepository,
        IGeminiService geminiService,
        IAiResponseParser responseParser,
        IMapper mapper,
        ILogger<AdminEssayGradingService> logger)
    {
        _submissionRepository = submissionRepository;
        _essayRepository = essayRepository;
        _assessmentRepository = assessmentRepository;
        _geminiService = geminiService;
        _responseParser = responseParser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResponse<EssayGradingResultDto>> GradeEssayWithAIAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        var response = new ServiceResponse<EssayGradingResultDto>();
        
        try
        {
            _logger.LogInformation("📝 Admin starting AI grading for submission {SubmissionId}", submissionId);

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
                response.Message = "Bài làm chỉ có file đính kèm. AI không thể chấm tự động.";
                return response;
            }

            var maxScore = essay.TotalPoints;

            // Use centralized prompt builder
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

            _logger.LogInformation("✅ AI grading completed for submission {SubmissionId}. Score: {Score}/{MaxScore}", 
                submissionId, aiResult.Score, maxScore);

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

    public async Task<ServiceResponse<EssayGradingResultDto>> GradeByAdminAsync(
        int submissionId, 
        TeacherGradingDto dto, 
        CancellationToken cancellationToken = default)
    {
        var response = new ServiceResponse<EssayGradingResultDto>();
        
        try
        {
            _logger.LogInformation("👨‍💼 Admin grading submission {SubmissionId}", submissionId);

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

            submission.TeacherScore = dto.Score;
            submission.TeacherFeedback = dto.Feedback;
            submission.GradedByTeacherId = null; // Admin không có teacherId
            submission.TeacherGradedAt = DateTime.UtcNow;
            submission.Status = SubmissionStatus.Graded;

            await _submissionRepository.UpdateSubmissionAsync(submission);

            _logger.LogInformation("✅ Admin grading completed for submission {SubmissionId}. Score: {Score}/{MaxScore}", 
                submissionId, dto.Score, maxScore);

            var result = _mapper.Map<EssayGradingResultDto>(submission);
            result.MaxScore = maxScore;

            response.Success = true;
            response.StatusCode = 200;
            response.Message = "Chấm điểm thành công bởi Admin";
            response.Data = result;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in admin grading for submission {SubmissionId}", submissionId);
            response.Success = false;
            response.StatusCode = 500;
            response.Message = "Có lỗi xảy ra khi chấm điểm";
            return response;
        }
    }

    public async Task<ServiceResponse<BatchGradingResultDto>> BatchGradeByAiAsync(int essayId, CancellationToken cancellationToken = default)
    {
            var response = new ServiceResponse<BatchGradingResultDto>();

            try
            {
                _logger.LogInformation("👨‍💼 Admin requesting batch AI grading for essay {EssayId}", essayId);

                // Get essay and assessment
                var essay = await _essayRepository.GetEssayByIdAsync(essayId);
                if (essay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài essay";
                    return response;
                }

                var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy assessment";
                    return response;
                }

                var maxScore = essay.TotalPoints;

                // Get all submissions chưa chấm (hoặc chỉ có AI score, chưa có teacher score)
                var allSubmissions = await _submissionRepository.GetSubmissionsByEssayIdAsync(essayId);
                var pendingSubmissions = allSubmissions
                    .Where(s => s.Status != SubmissionStatus.Graded || s.Score == null)
                    .Where(s => !string.IsNullOrWhiteSpace(s.TextContent)) // Only grade submissions with text
                    .ToList();

                _logger.LogInformation("Found {Count} submissions to grade", pendingSubmissions.Count);

                var results = new List<GradingResult>();
                int successCount = 0;
                int failCount = 0;

                foreach (var submission in pendingSubmissions)
                {
                    try
                    {
                        // Use centralized prompt builder
                        var prompt = EssayGradingPrompt.BuildPrompt(
                            essay.Title,
                            essay.Description ?? string.Empty,
                            submission.TextContent ?? string.Empty,
                            maxScore
                        );

                        // Call Gemini
                        var geminiResponse = await _geminiService.GenerateContentAsync(prompt, cancellationToken);

                        if (!geminiResponse.Success)
                        {
                            failCount++;
                            results.Add(new GradingResult
                            {
                                SubmissionId = submission.SubmissionId,
                                UserName = submission.User?.FullName ?? "Unknown",
                                Success = false,
                                Error = geminiResponse.ErrorMessage
                            });
                            continue;
                        }

                        // Use centralized response parser
                        var aiResult = _responseParser.ParseGradingResponse(geminiResponse.Content);

                        if (aiResult.Score > maxScore)
                        {
                            aiResult.Score = maxScore;
                        }

                        // Save result (AI score, NOT teacher score)
                        submission.Score = aiResult.Score;
                        submission.Feedback = aiResult.Feedback;
                        submission.GradedAt = DateTime.UtcNow;
                        submission.Status = SubmissionStatus.Graded;

                        await _submissionRepository.UpdateSubmissionAsync(submission);

                        successCount++;
                        results.Add(new GradingResult
                        {
                            SubmissionId = submission.SubmissionId,
                            UserName = submission.User?.FullName ?? "Unknown",
                            Score = aiResult.Score,
                            Success = true
                        });

                        _logger.LogInformation("✅ Graded submission {SubmissionId}: {Score}/{MaxScore}", submission.SubmissionId, aiResult.Score, maxScore);
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        _logger.LogError(ex, "❌ Error grading submission {SubmissionId}", submission.SubmissionId);
                        results.Add(new GradingResult
                        {
                            SubmissionId = submission.SubmissionId,
                            UserName = submission.User?.FullName ?? "Unknown",
                            Success = false,
                            Error = ex.Message
                        });
                    }
                }

                var batchResult = new BatchGradingResultDto
                {
                    TotalProcessed = pendingSubmissions.Count,
                    SuccessCount = successCount,
                    FailCount = failCount,
                    Results = results
                };

                response.Success = true;
                response.StatusCode = 200;
                response.Message = $"Chấm điểm AI hàng loạt hoàn tất: {successCount} thành công, {failCount} thất bại";
                response.Data = batchResult;

                _logger.LogInformation("✅ Batch AI grading completed for essay {EssayId}: {Success}/{Total}", essayId, successCount, pendingSubmissions.Count);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in batch AI grading for essay {EssayId}", essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi chấm điểm hàng loạt";
                return response;
            }
        }
    }
