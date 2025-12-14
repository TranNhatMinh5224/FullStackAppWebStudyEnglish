using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    /// <summary>
    /// AGGRESSIVE STRATEGY: Realtime-only pronunciation assessment
    /// - NO PronunciationAssessment entities stored in DB
    /// - ONLY PronunciationProgress (aggregated metrics) persisted
    /// - Minimizes DB storage for scalability
    /// </summary>
    public class PronunciationAssessmentService : IPronunciationAssessmentService
    {
        private readonly IFlashCardRepository _flashCardRepository;
        private readonly IMinioFileStorage _minioFileStorage;
        private readonly IAzureSpeechService _azureSpeechService;
        private readonly IPronunciationProgressRepository _progressRepository;
        private readonly ILogger<PronunciationAssessmentService> _logger;
        private const string BUCKET_NAME = "pronunciations";

        public PronunciationAssessmentService(
            IFlashCardRepository flashCardRepository,
            IMinioFileStorage minioFileStorage,
            IAzureSpeechService azureSpeechService,
            IPronunciationProgressRepository progressRepository,
            ILogger<PronunciationAssessmentService> logger)
        {
            _flashCardRepository = flashCardRepository;
            _minioFileStorage = minioFileStorage;
            _azureSpeechService = azureSpeechService;
            _progressRepository = progressRepository;
            _logger = logger;
        }

        /// <summary>
        /// Create realtime pronunciation assessment - NO persistence, only progress tracking
        /// </summary>
        public async Task<ServiceResponse<PronunciationAssessmentDto>> CreateAssessmentAsync(
            CreatePronunciationAssessmentDto dto,
            int userId)
        {
            var response = new ServiceResponse<PronunciationAssessmentDto>();

            try
            {
                _logger.LogInformation("Starting pronunciation assessment for user {UserId}, flashCard {FlashCardId}",
                    userId, dto.FlashCardId);

                // 1. Get FlashCard
                var flashCard = await _flashCardRepository.GetByIdAsync(dto.FlashCardId);
                if (flashCard == null)
                {
                    response.Success = false;
                    response.Message = "FlashCard not found";
                    return response;
                }

                var referenceText = flashCard.Word;
                if (string.IsNullOrWhiteSpace(referenceText))
                {
                    response.Success = false;
                    response.Message = "FlashCard does not have a valid word to assess";
                    return response;
                }

                // 2. Generate temp audio URL
                var tempAudioUrl = BuildPublicUrl.BuildURL(BUCKET_NAME, dto.AudioTempKey);

                // 3. Call Azure Speech Service - REALTIME assessment
                var azureResult = await _azureSpeechService.AssessPronunciationAsync(
                    tempAudioUrl,
                    referenceText);

                _logger.LogInformation("Azure result: Success={Success}, Score={Score}",
                    azureResult.Success, azureResult.PronunciationScore);

                // 4. Update ONLY PronunciationProgress (aggregated data)
                if (azureResult.Success)
                {
                    try
                    {
                        await _progressRepository.UpsertAsync(
                            userId,
                            dto.FlashCardId,
                            azureResult.AccuracyScore,
                            azureResult.FluencyScore,
                            azureResult.CompletenessScore,
                            azureResult.PronunciationScore,
                            azureResult.ProblemPhonemes,
                            azureResult.StrongPhonemes,
                            DateTime.UtcNow
                        );
                        _logger.LogInformation("Progress updated successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update progress");
                    }
                }

                // 5. Clean up temp audio
                try
                {
                    await _minioFileStorage.DeleteFileAsync(BUCKET_NAME, dto.AudioTempKey);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to delete temp audio: {Error}", ex.Message);
                }

                // 6. Build realtime response (not from DB)
                var resultDto = new PronunciationAssessmentDto
                {
                    UserId = userId,
                    FlashCardId = dto.FlashCardId,
                    ReferenceText = referenceText,
                    AudioUrl = string.Empty,
                    AudioType = dto.AudioType,
                    AudioSize = dto.AudioSize,
                    DurationInSeconds = dto.DurationInSeconds,

                    AccuracyScore = azureResult.AccuracyScore,
                    FluencyScore = azureResult.FluencyScore,
                    CompletenessScore = azureResult.CompletenessScore,
                    PronunciationScore = azureResult.PronunciationScore,

                    RecognizedText = azureResult.RecognizedText,
                    Feedback = azureResult.Success ? GenerateFeedback(azureResult) : azureResult.ErrorMessage,
                    Status = azureResult.Success ? "Completed" : "Failed",

                    Words = azureResult.Words,
                    ProblemPhonemes = azureResult.ProblemPhonemes,
                    StrongPhonemes = azureResult.StrongPhonemes,

                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    FlashCardWord = flashCard.Word
                };

                response.Success = true;
                response.Data = resultDto;
                response.Message = "Pronunciation assessed (realtime - not stored)";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assessment");
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Get flashcards with pronunciation progress for a module
        /// </summary>
        public async Task<ServiceResponse<List<FlashCardWithPronunciationDto>>> GetFlashCardsWithPronunciationProgressAsync(
            int moduleId,
            int userId)
        {
            var response = new ServiceResponse<List<FlashCardWithPronunciationDto>>();

            try
            {
                // Get all flashcards in module
                var flashCards = await _flashCardRepository.GetByModuleIdAsync(moduleId);

                // Get all pronunciation progress for this user in this module
                var progresses = await _progressRepository.GetByModuleIdAsync(userId, moduleId);
                var progressDict = progresses.ToDictionary(p => p.FlashCardId);

                // Map flashcards with progress
                var result = flashCards.Select(fc =>
                {
                    var hasProgress = progressDict.TryGetValue(fc.FlashCardId, out var progress);

                    PronunciationProgressSummary? progressSummary = null;
                    if (hasProgress && progress != null)
                    {
                        string status = "Practicing";
                        string statusColor = "yellow";

                        if (progress.IsMastered)
                        {
                            status = "Mastered";
                            statusColor = "green";
                        }
                        else if (progress.TotalAttempts == 0)
                        {
                            status = "Not Started";
                            statusColor = "gray";
                        }

                        progressSummary = new PronunciationProgressSummary
                        {
                            TotalAttempts = progress.TotalAttempts,
                            BestScore = progress.BestScore,
                            BestScoreDate = progress.BestScoreDate,
                            LastPracticedAt = progress.LastPracticedAt,
                            AvgPronunciationScore = progress.AvgPronunciationScore,
                            LastPronunciationScore = progress.LastPronunciationScore,
                            ConsecutiveDaysStreak = progress.ConsecutiveDaysStreak,
                            IsMastered = progress.IsMastered,
                            MasteredAt = progress.MasteredAt,
                            Status = status,
                            StatusColor = statusColor
                        };
                    }

                    return new FlashCardWithPronunciationDto
                    {
                        FlashCardId = fc.FlashCardId,
                        Word = fc.Word,
                        Definition = fc.Meaning,
                        Example = fc.Example,
                        ImageUrl = !string.IsNullOrWhiteSpace(fc.ImageKey)
                            ? BuildPublicUrl.BuildURL("flashcards", fc.ImageKey)
                            : null,
                        AudioUrl = !string.IsNullOrWhiteSpace(fc.AudioKey)
                            ? BuildPublicUrl.BuildURL("flashcards", fc.AudioKey)
                            : null,
                        Phonetic = fc.Pronunciation,
                        Progress = progressSummary
                    };
                }).ToList();

                response.Success = true;
                response.Data = result;
                response.Message = $"Retrieved {result.Count} flashcards with pronunciation progress";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flashcards with pronunciation progress");
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Get a single flashcard with pronunciation progress by index for practice mode
        /// </summary>
        public async Task<ServiceResponse<PaginatedFlashCardWithPronunciationDto>> GetFlashCardWithPronunciationByIndexAsync(
            int moduleId,
            int cardIndex,
            int userId)
        {
            var response = new ServiceResponse<PaginatedFlashCardWithPronunciationDto>();

            try
            {
                // Validate cardIndex
                if (cardIndex < 1)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Chỉ số thẻ phải lớn hơn 0";
                    return response;
                }

                // Get total count
                var totalCards = await _flashCardRepository.GetFlashCardCountByModuleAsync(moduleId);

                if (totalCards == 0)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Module không có flashcard nào";
                    return response;
                }

                // Validate cardIndex against total
                if (cardIndex > totalCards)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Chỉ số thẻ vượt quá tổng số thẻ ({totalCards})";
                    return response;
                }

                // Get the single flashcard at the index
                var flashCard = await _flashCardRepository.GetSingleFlashCardByModuleAsync(moduleId, cardIndex);

                if (flashCard == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy flashcard";
                    return response;
                }

                // Get pronunciation progress for this flashcard
                var progress = await _progressRepository.GetByUserAndFlashCardAsync(userId, flashCard.FlashCardId);

                PronunciationProgressSummary? progressSummary = null;
                if (progress != null)
                {
                    string status = "Practicing";
                    string statusColor = "yellow";

                    if (progress.IsMastered)
                    {
                        status = "Mastered";
                        statusColor = "green";
                    }
                    else if (progress.TotalAttempts == 0)
                    {
                        status = "Not Started";
                        statusColor = "gray";
                    }

                    progressSummary = new PronunciationProgressSummary
                    {
                        TotalAttempts = progress.TotalAttempts,
                        BestScore = progress.BestScore,
                        BestScoreDate = progress.BestScoreDate,
                        LastPracticedAt = progress.LastPracticedAt,
                        AvgPronunciationScore = progress.AvgPronunciationScore,
                        LastPronunciationScore = progress.LastPronunciationScore,
                        ConsecutiveDaysStreak = progress.ConsecutiveDaysStreak,
                        IsMastered = progress.IsMastered,
                        MasteredAt = progress.MasteredAt,
                        Status = status,
                        StatusColor = statusColor
                    };
                }

                var flashCardDto = new FlashCardWithPronunciationDto
                {
                    FlashCardId = flashCard.FlashCardId,
                    Word = flashCard.Word,
                    Definition = flashCard.Meaning,
                    Example = flashCard.Example,
                    ImageUrl = !string.IsNullOrWhiteSpace(flashCard.ImageKey)
                        ? BuildPublicUrl.BuildURL("flashcards", flashCard.ImageKey)
                        : null,
                    AudioUrl = !string.IsNullOrWhiteSpace(flashCard.AudioKey)
                        ? BuildPublicUrl.BuildURL("flashcards", flashCard.AudioKey)
                        : null,
                    Phonetic = flashCard.Pronunciation,
                    Progress = progressSummary
                };

                // Create paginated response
                response.Data = new PaginatedFlashCardWithPronunciationDto
                {
                    FlashCard = flashCardDto,
                    CurrentIndex = cardIndex,
                    TotalCards = totalCards
                };

                response.Success = true;
                response.Message = $"Lấy flashcard {cardIndex}/{totalCards} để luyện phát âm thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flashcard with pronunciation by index: {CardIndex}, ModuleId: {ModuleId}", 
                    cardIndex, moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy flashcard để luyện phát âm";
            }

            return response;
        }

        private static string GenerateFeedback(AzureSpeechAssessmentResult result)
        {
            var feedback = new List<string>();

            // Overall assessment
            if (result.PronunciationScore >= 90)
                feedback.Add("🌟 Excellent pronunciation! Keep it up!");
            else if (result.PronunciationScore >= 75)
                feedback.Add("✅ Good job! You're doing well.");
            else if (result.PronunciationScore >= 60)
                feedback.Add("📚 Not bad, but there's room for improvement.");
            else
                feedback.Add("💪 Keep practicing! You'll get better.");

            // Specific feedback
            if (result.AccuracyScore < 70)
                feedback.Add($"⚠️ Focus on accuracy (current: {result.AccuracyScore:F1}%). Practice the correct pronunciation.");

            if (result.FluencyScore < 70)
                feedback.Add($"🗣️ Work on fluency (current: {result.FluencyScore:F1}%). Try speaking more naturally.");

            if (result.CompletenessScore < 90)
                feedback.Add($"📝 Completeness needs work (current: {result.CompletenessScore:F1}%). Make sure to pronounce the full word.");

            // Problem phonemes
            if (result.ProblemPhonemes.Count != 0)
            {
                var phonemeList = string.Join(", ", result.ProblemPhonemes.Take(3));
                feedback.Add($"🎯 Focus on these sounds: {phonemeList}");
            }

            return string.Join(" ", feedback);
        }
    }
}
