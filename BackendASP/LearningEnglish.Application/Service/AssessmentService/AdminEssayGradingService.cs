using AutoMapper;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LearningEnglish.Application.Service.EssayGrading;

public class AdminEssayGradingService : IAdminEssayGradingService
{
    private readonly IEssaySubmissionRepository _submissionRepository;
    private readonly IEssayRepository _essayRepository;
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IGeminiService _geminiService;
    private readonly IMapper _mapper;
    private readonly ILogger<AdminEssayGradingService> _logger;

    public AdminEssayGradingService(
        IEssaySubmissionRepository submissionRepository,
        IEssayRepository essayRepository,
        IAssessmentRepository assessmentRepository,
        IGeminiService geminiService,
        IMapper mapper,
        ILogger<AdminEssayGradingService> logger)
    {
        _submissionRepository = submissionRepository;
        _essayRepository = essayRepository;
        _assessmentRepository = assessmentRepository;
        _geminiService = geminiService;
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

            var maxScore = assessment.TotalPoints;

            var prompt = BuildGradingPrompt(
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

            var aiResult = ParseAiResponse(geminiResponse.Content);

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

            var maxScore = assessment.TotalPoints;

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

    private string BuildGradingPrompt(string question, string description, string studentEssay, decimal maxScore)
    {
        return $@"You are an expert English essay grader. Grade the following student essay according to the rubric below.

ESSAY QUESTION:
{question}

{(string.IsNullOrWhiteSpace(description) ? "" : $"DESCRIPTION:\n{description}\n")}
MAX SCORE: {maxScore} points

STUDENT ESSAY:
{studentEssay}

GRADING RUBRIC:
1. Content & Ideas (40%):
   - Thesis statement (10%)
   - Supporting arguments (15%)
   - Examples and evidence (10%)
   - Conclusion (5%)

2. Language Use (30%):
   - Vocabulary range and accuracy (15%)
   - Grammar and sentence structure (15%)

3. Organization (20%):
   - Essay structure and flow (10%)
   - Coherence and cohesion (10%)

4. Mechanics (10%):
   - Spelling, punctuation, capitalization

INSTRUCTIONS:
- Provide a score out of {maxScore} points
- Give detailed feedback on each rubric category
- List specific strengths (at least 2)
- List specific areas for improvement (at least 2)
- Be constructive and encouraging

OUTPUT FORMAT (JSON only, no other text):
{{
    ""score"": <numeric score>,
    ""feedback"": ""<overall feedback>"",
    ""breakdown"": {{
        ""contentScore"": <score for content>,
        ""languageScore"": <score for language>,
        ""organizationScore"": <score for organization>,
        ""mechanicsScore"": <score for mechanics>
    }},
    ""strengths"": [""<strength 1>"", ""<strength 2>""],
    ""improvements"": [""<improvement 1>"", ""<improvement 2>""]
}}";
    }

    private AiGradingResult ParseAiResponse(string content)
    {
        try
        {
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            
            if (jsonStart == -1 || jsonEnd == -1)
            {
                throw new Exception("No JSON found in AI response");
            }

            var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<AiGradingResult>(jsonContent, options);

            if (result == null)
            {
                throw new Exception("Failed to deserialize AI response");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse AI response: {Content}", content);
            
            return new AiGradingResult
            {
                Score = 0,
                Feedback = "Error parsing AI response. Please grade manually.",
                Strengths = new List<string> { "Unable to analyze" },
                Improvements = new List<string> { "Unable to analyze" }
            };
        }
    }
}
