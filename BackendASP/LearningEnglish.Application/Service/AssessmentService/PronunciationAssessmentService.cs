
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
        private readonly IFlashCardRepository _flashCardRepository;
        private readonly IMinioFileStorage _minioFileStorage;
        private readonly IAzureSpeechService _azureSpeechService;
        private readonly IPronunciationProgressRepository _progressRepository;
        private readonly IMapper _mapper;
        private const string BUCKET_NAME = "pronunciations";

        public PronunciationAssessmentService(
            IPronunciationAssessmentRepository repository,
            IFlashCardRepository flashCardRepository,
            IMinioFileStorage minioFileStorage,
            IAzureSpeechService azureSpeechService,
            IPronunciationProgressRepository progressRepository,
            IMapper mapper)
        {
            _repository = repository;
            _flashCardRepository = flashCardRepository;
            _minioFileStorage = minioFileStorage;
            _azureSpeechService = azureSpeechService;
            _progressRepository = progressRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<PronunciationAssessmentDto>> CreateAssessmentAsync(
            CreatePronunciationAssessmentDto dto,
            int userId)
        {
            var response = new ServiceResponse<PronunciationAssessmentDto>();

            try
            {
                // 1. Get FlashCard to retrieve reference text
                var flashCard = await _flashCardRepository.GetByIdAsync(dto.FlashCardId);
                if (flashCard == null)
                {
                    response.Success = false;
                    response.Message = "FlashCard not found";
                    return response;
                }

                var referenceText = flashCard.Word; // Use the Word as reference text

                if (string.IsNullOrWhiteSpace(referenceText))
                {
                    response.Success = false;
                    response.Message = "FlashCard does not have a valid word to assess";
                    return response;
                }

                // 2. Commit audio from temp to real
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
                    // 3. Generate public URL
                    var audioUrl = BuildPublicUrl.BuildURL(BUCKET_NAME, audioKey);

                    // 4. Create entity with Pending status
                    var assessment = new PronunciationAssessment
                    {
                        UserId = userId,
                        FlashCardId = dto.FlashCardId,
                        AssessmentId = null, // Can be linked to Assessment if needed
                        ReferenceText = referenceText, // From FlashCard.Word
                        AudioKey = audioKey,
                        AudioType = dto.AudioType,
                        AudioSize = dto.AudioSize,
                        DurationInSeconds = dto.DurationInSeconds,
                        Status = AssessmentStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // 5. Save to DB
                    var savedAssessment = await _repository.CreateAsync(assessment);

                    // 6. Start Azure assessment (change status to Processing)
                    savedAssessment.Status = AssessmentStatus.Processing;
                    savedAssessment.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(savedAssessment);

                    // 7. Call Azure Speech Service
                    var azureResult = await _azureSpeechService.AssessPronunciationAsync(
                        audioUrl,
                        referenceText); // Use referenceText from FlashCard

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

                        // 🆕 Serialize and save word-level data
                        savedAssessment.WordsDataJson = System.Text.Json.JsonSerializer.Serialize(azureResult.Words);
                        savedAssessment.ProblemPhonemesJson = System.Text.Json.JsonSerializer.Serialize(azureResult.ProblemPhonemes);
                        savedAssessment.StrongPhonemesJson = System.Text.Json.JsonSerializer.Serialize(azureResult.StrongPhonemes);

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

                    // 🆕 8. Update PronunciationProgress (only if assessment completed successfully)
                    if (savedAssessment.Status == AssessmentStatus.Completed && savedAssessment.FlashCardId.HasValue)
                    {
                        try
                        {
                            await _progressRepository.UpsertAsync(userId, savedAssessment.FlashCardId.Value, savedAssessment);
                        }
                        catch
                        {
                            // Progress update failed, but main assessment succeeded - don't fail request
                        }
                    }

                    // 9. Map to DTO
                    var resultDto = _mapper.Map<PronunciationAssessmentDto>(savedAssessment);
                    resultDto.AudioUrl = audioUrl; // Use public URL

                    // 🆕 Include word-level details in response
                    resultDto.Words = azureResult.Words;
                    resultDto.ProblemPhonemes = azureResult.ProblemPhonemes;
                    resultDto.StrongPhonemes = azureResult.StrongPhonemes;

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
                dto.AudioUrl = BuildPublicUrl.BuildURL(BUCKET_NAME, assessment.AudioKey);

                // 🆕 Deserialize word-level data if available
                if (!string.IsNullOrEmpty(assessment.WordsDataJson))
                {
                    try
                    {
                        dto.Words = System.Text.Json.JsonSerializer.Deserialize<List<WordPronunciationDetail>>(assessment.WordsDataJson) ?? new();
                    }
                    catch { dto.Words = new(); }
                }

                if (!string.IsNullOrEmpty(assessment.ProblemPhonemesJson))
                {
                    try
                    {
                        dto.ProblemPhonemes = System.Text.Json.JsonSerializer.Deserialize<List<string>>(assessment.ProblemPhonemesJson) ?? new();
                    }
                    catch { dto.ProblemPhonemes = new(); }
                }

                if (!string.IsNullOrEmpty(assessment.StrongPhonemesJson))
                {
                    try
                    {
                        dto.StrongPhonemes = System.Text.Json.JsonSerializer.Deserialize<List<string>>(assessment.StrongPhonemesJson) ?? new();
                    }
                    catch { dto.StrongPhonemes = new(); }
                }

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
                await _minioFileStorage.DeleteFileAsync(BUCKET_NAME, assessment.AudioKey);

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

        // 🆕 Enhanced feedback generation with word-level insights
        private string GenerateFeedback(AzureSpeechAssessmentResult result)
        {
            var feedback = new List<string>();

            // Overall assessment with emojis
            if (result.PronunciationScore >= 90)
                feedback.Add("🎉 Excellent! Your pronunciation is nearly perfect.");
            else if (result.PronunciationScore >= 75)
                feedback.Add("👍 Good job! Just a few minor improvements needed.");
            else if (result.PronunciationScore >= 60)
                feedback.Add("💪 Fair! Keep practicing the highlighted areas.");
            else
                feedback.Add("📚 Keep going! Regular practice will help you improve.");

            // 🆕 Word-specific feedback
            var weakWords = result.Words?
                .Where(w => w.AccuracyScore < 60)
                .OrderBy(w => w.AccuracyScore)
                .Take(3)
                .ToList();

            if (weakWords?.Any() == true)
            {
                var wordList = string.Join(", ", weakWords.Select(w => $"\"{w.Word}\" ({w.AccuracyScore:F0}%)"));
                feedback.Add($"\n🎯 Focus on these words: {wordList}");
            }

            // 🆕 Phoneme-specific guidance
            if (result.ProblemPhonemes.Any())
            {
                feedback.Add($"\n🔤 Practice these sounds: {string.Join(", ", result.ProblemPhonemes.Take(3))}");

                // Specific tips for common problem phonemes
                foreach (var phoneme in result.ProblemPhonemes.Take(2))
                {
                    var tip = GetPhonemeTip(phoneme);
                    if (!string.IsNullOrEmpty(tip))
                        feedback.Add($"\n💡 {tip}");
                }
            }

            // Component-specific tips
            if (result.FluencyScore < 70)
                feedback.Add("\n⏱️ Tip: Try speaking at a steady, natural pace without long pauses.");

            if (result.CompletenessScore < 70)
                feedback.Add("\n📝 Tip: Make sure to pronounce all words clearly and completely.");

            if (result.AccuracyScore < 70)
                feedback.Add("\n🎵 Tip: Focus on pronouncing each sound clearly and distinctly.");

            // 🆕 Positive reinforcement
            if (result.StrongPhonemes.Any())
            {
                feedback.Add($"\n✨ You're great at: {string.Join(", ", result.StrongPhonemes.Take(3))}!");
            }

            return string.Join(" ", feedback);
        }

        // 🆕 Get specific pronunciation tips for common problem phonemes
        private string GetPhonemeTip(string phoneme)
        {
            var tips = new Dictionary<string, string>
            {
                { "th", "For 'th' sound: Place your tongue between your teeth and blow air gently." },
                { "er", "For 'er' sound: Curl your tongue back without touching the roof of your mouth." },
                { "r", "For 'r' sound: Curl your tongue back and make it tense." },
                { "l", "For 'l' sound: Touch the roof of your mouth with your tongue tip." },
                { "v", "For 'v' sound: Touch your upper teeth to your lower lip and vibrate." },
                { "w", "For 'w' sound: Round your lips as if saying 'oo', then quickly open." },
                { "sh", "For 'sh' sound: Put your lips forward in a small circle." },
                { "zh", "For 'zh' sound: Like 'sh' but with voice (as in 'measure')." },
                { "ng", "For 'ng' sound: Block air with back of tongue against soft palate." },
                { "j", "For 'j' sound: Like 'dzh' - 'd' + 'zh' together." },
                { "ch", "For 'ch' sound: Like 'tsh' - 't' + 'sh' together." },
                { "a", "For short 'a' sound: Open mouth wide, tongue flat and low." },
                { "i", "For long 'i' sound: Start with 'ah' and glide to 'ee'." },
                { "ow", "For 'ow' sound: Start with 'ah' and round lips to 'oo'." }
            };

            return tips.TryGetValue(phoneme, out var tip) ? tip : "";
        }

        // 🆕 Get progress analytics over time
        public async Task<ServiceResponse<ProgressAnalytics>> GetProgressAnalyticsAsync(
            int userId,
            int months = 3)
        {
            var response = new ServiceResponse<ProgressAnalytics>();

            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                var assessments = await _repository.GetByUserIdSinceDateAsync(userId, startDate);
                var completed = assessments
                    .Where(a => a.Status == AssessmentStatus.Completed)
                    .OrderBy(a => a.CreatedAt)
                    .ToList();

                if (!completed.Any())
                {
                    response.Success = false;
                    response.Message = "No completed assessments found in this period";
                    return response;
                }

                // Chart data - group by week
                var chartData = completed
                    .GroupBy(a => new
                    {
                        Year = a.CreatedAt.Year,
                        Week = System.Globalization.ISOWeek.GetWeekOfYear(a.CreatedAt)
                    })
                    .Select(g => new ProgressDataPoint
                    {
                        Date = System.Globalization.ISOWeek.ToDateTime(g.Key.Year, g.Key.Week, DayOfWeek.Monday),
                        AverageScore = g.Average(a => a.PronunciationScore),
                        AssessmentsCount = g.Count()
                    })
                    .OrderBy(p => p.Date)
                    .ToList();

                // Milestones
                var milestones = new List<Milestone>();

                if (completed.Count == 1)
                    milestones.Add(new Milestone
                    {
                        Date = completed.First().CreatedAt,
                        Achievement = "First assessment completed!",
                        Score = completed.First().PronunciationScore,
                        Icon = "🎯"
                    });

                if (completed.Count >= 10)
                    milestones.Add(new Milestone
                    {
                        Date = completed[9].CreatedAt,
                        Achievement = "10 assessments milestone!",
                        Score = completed.Take(10).Average(a => a.PronunciationScore),
                        Icon = "📚"
                    });

                if (completed.Count >= 50)
                    milestones.Add(new Milestone
                    {
                        Date = completed[49].CreatedAt,
                        Achievement = "50 assessments milestone!",
                        Score = completed.Take(50).Average(a => a.PronunciationScore),
                        Icon = "🏆"
                    });

                if (completed.Count >= 100)
                    milestones.Add(new Milestone
                    {
                        Date = completed[99].CreatedAt,
                        Achievement = "100 assessments - You're dedicated!",
                        Score = completed.Take(100).Average(a => a.PronunciationScore),
                        Icon = "🌟"
                    });

                // Level milestones
                var firstScore = completed.First().PronunciationScore;
                var currentScore = completed.TakeLast(10).Average(a => a.PronunciationScore);

                if (currentScore >= 70 && firstScore < 70)
                {
                    var levelUpAssessment = completed.First(a => a.PronunciationScore >= 70);
                    milestones.Add(new Milestone
                    {
                        Date = levelUpAssessment.CreatedAt,
                        Achievement = "Reached Intermediate level!",
                        Score = 70,
                        Icon = "⭐"
                    });
                }

                if (currentScore >= 85 && firstScore < 85)
                {
                    var levelUpAssessment = completed.First(a => a.PronunciationScore >= 85);
                    milestones.Add(new Milestone
                    {
                        Date = levelUpAssessment.CreatedAt,
                        Achievement = "Reached Advanced level!",
                        Score = 85,
                        Icon = "🚀"
                    });
                }

                // Overall improvement
                var overallImprovement = ((currentScore - firstScore) / firstScore) * 100;

                // Progress summary
                var progressSummary = GenerateProgressSummary(firstScore, currentScore, completed.Count, months);

                // Phoneme progress analysis
                var phonemeProgress = AnalyzePhonemeProgress(completed);

                var analytics = new ProgressAnalytics
                {
                    ChartData = chartData,
                    Milestones = milestones.OrderBy(m => m.Date).ToList(),
                    PhonemeProgress = phonemeProgress,
                    OverallImprovementPercent = Math.Round(overallImprovement, 1),
                    ProgressSummary = progressSummary
                };

                response.Success = true;
                response.Data = analytics;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error getting progress analytics: {ex.Message}";
            }

            return response;
        }

        // Helper: Calculate percentile
        private int CalculatePercentile(double userScore, List<double> allScores)
        {
            if (!allScores.Any()) return 50;
            var lowerCount = allScores.Count(s => s < userScore);
            return (int)Math.Round((double)lowerCount / allScores.Count * 100);
        }

        // Helper: Generate progress summary
        private string GenerateProgressSummary(double firstScore, double currentScore, int totalCount, int months)
        {
            var improvement = currentScore - firstScore;
            var avgPerMonth = improvement / months;

            if (improvement > 15)
                return $"🚀 Excellent progress! You've improved {improvement:F1} points in {months} months. Keep up the amazing work!";
            else if (improvement > 5)
                return $"📈 Good progress! You've improved {improvement:F1} points. Stay consistent and you'll reach the next level soon!";
            else if (improvement > 0)
                return $"📊 Steady progress! You've improved {improvement:F1} points. Keep practicing regularly for better results.";
            else
                return $"💪 Keep practicing! Consistency is key. Try to practice more regularly for improvement.";
        }

        // Helper: Analyze phoneme progress
        private PhonemeProgressAnalysis AnalyzePhonemeProgress(List<PronunciationAssessment> assessments)
        {
            try
            {
                // Track phoneme scores over time
                var phonemeScoresOverTime = new Dictionary<string, List<(DateTime date, double score)>>();

                foreach (var assessment in assessments.Where(a => !string.IsNullOrEmpty(a.WordsDataJson)))
                {
                    try
                    {
                        var words = System.Text.Json.JsonSerializer.Deserialize<List<WordPronunciationDetail>>(assessment.WordsDataJson!);
                        if (words == null) continue;

                        foreach (var word in words)
                        {
                            foreach (var phoneme in word.Phonemes)
                            {
                                var displayPhoneme = phoneme.PhonemeDisplay;
                                if (string.IsNullOrEmpty(displayPhoneme)) continue;

                                if (!phonemeScoresOverTime.ContainsKey(displayPhoneme))
                                    phonemeScoresOverTime[displayPhoneme] = new();

                                phonemeScoresOverTime[displayPhoneme].Add((assessment.CreatedAt, phoneme.AccuracyScore));
                            }
                        }
                    }
                    catch { continue; }
                }

                // Calculate improvement for each phoneme
                var phonemeImprovements = new List<PhonemeImprovement>();

                foreach (var kvp in phonemeScoresOverTime.Where(p => p.Value.Count >= 3))
                {
                    var phoneme = kvp.Key;
                    var scores = kvp.Value.OrderBy(s => s.date).ToList();

                    // Compare first 30% with last 30%
                    var earlyCount = Math.Max(1, scores.Count / 3);
                    var lateCount = Math.Max(1, scores.Count / 3);

                    var earlyAvg = scores.Take(earlyCount).Average(s => s.score);
                    var lateAvg = scores.TakeLast(lateCount).Average(s => s.score);
                    var improvement = lateAvg - earlyAvg;
                    var improvementPercent = earlyAvg > 0 ? (improvement / earlyAvg) * 100 : 0;

                    phonemeImprovements.Add(new PhonemeImprovement
                    {
                        Phoneme = phoneme,
                        PhonemeDisplay = phoneme,
                        FromScore = Math.Round(earlyAvg, 1),
                        ToScore = Math.Round(lateAvg, 1),
                        ImprovementPercent = Math.Round(improvementPercent, 1),
                        OccurrenceCount = scores.Count
                    });
                }

                // Get most improved (positive improvement)
                var mostImproved = phonemeImprovements
                    .Where(p => p.ImprovementPercent > 0)
                    .OrderByDescending(p => p.ImprovementPercent)
                    .Take(5)
                    .ToList();

                // Get needs work (negative or low scores)
                var needsWork = phonemeImprovements
                    .Where(p => p.ToScore < 70 || p.ImprovementPercent < 0)
                    .OrderBy(p => p.ToScore)
                    .Take(5)
                    .ToList();

                return new PhonemeProgressAnalysis
                {
                    MostImproved = mostImproved,
                    NeedsWork = needsWork
                };
            }
            catch (Exception)
            {
                // Return empty analysis if parsing fails
                return new PhonemeProgressAnalysis
                {
                    MostImproved = new List<PhonemeImprovement>(),
                    NeedsWork = new List<PhonemeImprovement>()
                };
            }
        }
    }
}
