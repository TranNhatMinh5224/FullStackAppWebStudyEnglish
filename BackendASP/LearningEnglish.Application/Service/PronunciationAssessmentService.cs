using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service
{
    public class PronunciationAssessmentService : IPronunciationAssessmentService
    {
        private readonly IPronunciationAssessmentRepository _repository;
        private readonly IMinioFileStorage _minioFileStorage;
        private readonly IAzureSpeechService _azureSpeechService;
        private readonly IMapper _mapper;
        private const string BUCKET_NAME = "pronunciations";

        public PronunciationAssessmentService(
            IPronunciationAssessmentRepository repository,
            IMinioFileStorage minioFileStorage,
            IAzureSpeechService azureSpeechService,
            IMapper mapper)
        {
            _repository = repository;
            _minioFileStorage = minioFileStorage;
            _azureSpeechService = azureSpeechService;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<PronunciationAssessmentDto>> CreateAssessmentAsync(
            CreatePronunciationAssessmentDto dto, 
            int userId)
        {
            var response = new ServiceResponse<PronunciationAssessmentDto>();

            try
            {
                // 1. Commit audio from temp to real
                var commitResult = await _minioFileStorage.CommitFileAsync(
                    dto.AudioTempKey, 
                    BUCKET_NAME, 
                    "real");

                if (!commitResult.Success || string.IsNullOrEmpty(commitResult.Data))
                {
                    response.Success = false;
                    response.Message = commitResult.Message ?? "Failed to commit audio file";
                    return response;
                }

                var audioKey = commitResult.Data;

                try
                {
                    // 2. Generate public URL
                    var audioUrl = BuildPublicUrl.BuildURL(BUCKET_NAME, audioKey);

                    // 3. Create entity with Pending status
                    var assessment = new PronunciationAssessment
                    {
                        UserId = userId,
                        FlashCardId = dto.FlashCardId,
                        AssignmentId = dto.AssignmentId,
                        ReferenceText = dto.ReferenceText,
                        AudioUrl = audioKey,
                        AudioType = dto.AudioType,
                        AudioSize = dto.AudioSize,
                        DurationInSeconds = dto.DurationInSeconds,
                        Status = AssessmentStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // 4. Save to DB
                    var savedAssessment = await _repository.CreateAsync(assessment);

                    // 5. Start Azure assessment (change status to Processing)
                    savedAssessment.Status = AssessmentStatus.Processing;
                    savedAssessment.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(savedAssessment);

                    // 6. Call Azure Speech Service
                    var azureResult = await _azureSpeechService.AssessPronunciationAsync(
                        audioUrl, 
                        dto.ReferenceText);

                    if (azureResult.Success)
                    {
                        // Update with scores
                        savedAssessment.AccuracyScore = azureResult.AccuracyScore;
                        savedAssessment.FluencyScore = azureResult.FluencyScore;
                        savedAssessment.CompletenessScore = azureResult.CompletenessScore;
                        savedAssessment.PronunciationScore = azureResult.PronunciationScore;
                        savedAssessment.RecognizedText = azureResult.RecognizedText;
                        savedAssessment.DetailedResultJson = azureResult.DetailedResultJson;
                        savedAssessment.AzureRawResponse = azureResult.RawResponse;
                        savedAssessment.Feedback = GenerateFeedback(azureResult);
                        savedAssessment.Status = AssessmentStatus.Completed;
                        savedAssessment.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Assessment failed
                        savedAssessment.Status = AssessmentStatus.Failed;
                        savedAssessment.Feedback = azureResult.ErrorMessage;
                        savedAssessment.UpdatedAt = DateTime.UtcNow;
                    }

                    await _repository.UpdateAsync(savedAssessment);

                    // 7. Map to DTO
                    var resultDto = _mapper.Map<PronunciationAssessmentDto>(savedAssessment);
                    resultDto.AudioUrl = audioUrl; // Use public URL

                    response.Success = true;
                    response.Data = resultDto;
                    response.Message = azureResult.Success 
                        ? "Pronunciation assessed successfully" 
                        : "Audio uploaded but assessment failed";
                }
                catch
                {
                    // Rollback: Delete audio file from MinIO
                    await _minioFileStorage.DeleteFileAsync(BUCKET_NAME, audioKey);
                    throw;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating assessment: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<PronunciationAssessmentDto>> GetAssessmentByIdAsync(int id, int userId)
        {
            var response = new ServiceResponse<PronunciationAssessmentDto>();

            try
            {
                var assessment = await _repository.GetByIdAsync(id);

                if (assessment == null)
                {
                    response.Success = false;
                    response.Message = "Assessment not found";
                    return response;
                }

                if (assessment.UserId != userId)
                {
                    response.Success = false;
                    response.Message = "Access denied";
                    return response;
                }

                var dto = _mapper.Map<PronunciationAssessmentDto>(assessment);
                dto.AudioUrl = BuildPublicUrl.BuildURL(BUCKET_NAME, assessment.AudioUrl);

                response.Success = true;
                response.Data = dto;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving assessment: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<List<ListPronunciationAssessmentDto>>> GetUserAssessmentsAsync(int userId)
        {
            var response = new ServiceResponse<List<ListPronunciationAssessmentDto>>();

            try
            {
                var assessments = await _repository.GetByUserIdAsync(userId);
                var dtos = _mapper.Map<List<ListPronunciationAssessmentDto>>(assessments);

                response.Success = true;
                response.Data = dtos;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving assessments: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<List<ListPronunciationAssessmentDto>>> GetFlashCardAssessmentsAsync(
            int flashCardId, 
            int userId)
        {
            var response = new ServiceResponse<List<ListPronunciationAssessmentDto>>();

            try
            {
                var assessments = await _repository.GetByFlashCardIdAsync(flashCardId);
                
                // Filter by userId for security
                var userAssessments = assessments.Where(a => a.UserId == userId).ToList();
                var dtos = _mapper.Map<List<ListPronunciationAssessmentDto>>(userAssessments);

                response.Success = true;
                response.Data = dtos;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving flashcard assessments: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteAssessmentAsync(int id, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var assessment = await _repository.GetByIdAsync(id);

                if (assessment == null)
                {
                    response.Success = false;
                    response.Message = "Assessment not found";
                    return response;
                }

                if (assessment.UserId != userId)
                {
                    response.Success = false;
                    response.Message = "Access denied";
                    return response;
                }

                // Delete audio file from MinIO
                await _minioFileStorage.DeleteFileAsync(BUCKET_NAME, assessment.AudioUrl);

                // Delete from DB
                await _repository.DeleteAsync(id);

                response.Success = true;
                response.Data = true;
                response.Message = "Assessment deleted successfully";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting assessment: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<object>> GetUserStatisticsAsync(int userId)
        {
            var response = new ServiceResponse<object>();

            try
            {
                var assessments = await _repository.GetByUserIdAsync(userId);
                var completedAssessments = assessments
                    .Where(a => a.Status == AssessmentStatus.Completed)
                    .ToList();

                var stats = new
                {
                    TotalAssessments = assessments.Count,
                    CompletedAssessments = completedAssessments.Count,
                    AverageAccuracy = completedAssessments.Any() 
                        ? completedAssessments.Average(a => a.AccuracyScore) 
                        : 0,
                    AverageFluency = completedAssessments.Any() 
                        ? completedAssessments.Average(a => a.FluencyScore) 
                        : 0,
                    AverageCompleteness = completedAssessments.Any() 
                        ? completedAssessments.Average(a => a.CompletenessScore) 
                        : 0,
                    AveragePronunciation = completedAssessments.Any() 
                        ? completedAssessments.Average(a => a.PronunciationScore) 
                        : 0,
                    RecentAssessments = completedAssessments
                        .OrderByDescending(a => a.CreatedAt)
                        .Take(5)
                        .Select(a => new
                        {
                            a.PronunciationAssessmentId,
                            a.ReferenceText,
                            a.PronunciationScore,
                            a.CreatedAt
                        })
                        .ToList()
                };

                response.Success = true;
                response.Data = stats;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving statistics: {ex.Message}";
            }

            return response;
        }

        private string GenerateFeedback(AzureSpeechAssessmentResult result)
        {
            var feedback = new List<string>();

            if (result.PronunciationScore >= 90)
                feedback.Add("Excellent pronunciation!");
            else if (result.PronunciationScore >= 75)
                feedback.Add("Good pronunciation with minor areas for improvement.");
            else if (result.PronunciationScore >= 60)
                feedback.Add("Fair pronunciation. Practice more to improve clarity.");
            else
                feedback.Add("Needs significant improvement. Keep practicing!");

            if (result.AccuracyScore < 70)
                feedback.Add("Focus on pronouncing individual sounds more accurately.");

            if (result.FluencyScore < 70)
                feedback.Add("Try to speak more smoothly without long pauses.");

            if (result.CompletenessScore < 70)
                feedback.Add("Make sure to pronounce all words in the text.");

            return string.Join(" ", feedback);
        }
    }
}
