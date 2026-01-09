using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.Application.Interface.Strategies;
using AutoMapper;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;


namespace LearningEnglish.Application.Service
{
    public class QuizAttemptService : IQuizAttemptService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly IMapper _mapper;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEnumerable<IScoringStrategy> _scoringStrategies;
        private readonly IQuestionRepository _questionRepository;
        private readonly IModuleProgressService _moduleProgressService;
        private readonly IStreakService _streakService;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<QuizAttemptService> _logger;
        private readonly IQuizAttemptMapper _quizAttemptMapper;


        public QuizAttemptService(
            IQuizRepository quizRepository,
            IQuizAttemptRepository quizAttemptRepository,
            IMapper mapper,
            IAssessmentRepository assessmentRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IEnumerable<IScoringStrategy> scoringStrategies,
            IQuestionRepository questionRepository,
            IModuleProgressService moduleProgressService,
            IStreakService streakService,
            INotificationRepository notificationRepository,
            ILogger<QuizAttemptService> logger,
            IQuizAttemptMapper quizAttemptMapper)
        {
            _quizRepository = quizRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _mapper = mapper;
            _assessmentRepository = assessmentRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _scoringStrategies = scoringStrategies;
            _questionRepository = questionRepository;
            _moduleProgressService = moduleProgressService;
            _streakService = streakService;
            _notificationRepository = notificationRepository;
            _logger = logger;
            _quizAttemptMapper = quizAttemptMapper;

            

        }

        public async Task<ServiceResponse<QuizAttemptWithQuestionsDto>> StartQuizAttemptAsync(int quizId, int userId)
        {
            var response = new ServiceResponse<QuizAttemptWithQuestionsDto>();

            try
            {
                // 1. Kiểm tra quiz tồn tại + trạng thái
                var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = "Quiz không tồn tại";
                    response.StatusCode = 404;
                    return response;
                }

                if (quiz.Status == QuizStatus.Closed)
                {
                    response.Success = false;
                    response.Message = "Quiz đã đóng";
                    response.StatusCode = 403;
                    return response;
                }

                // (Optional) kiểm tra thời gian mở quiz
                if (quiz.AvailableFrom.HasValue && DateTime.UtcNow < quiz.AvailableFrom.Value)
                {
                    response.Success = false;
                    response.Message = "Quiz chưa mở để làm";
                    response.StatusCode = 403;
                    return response;
                }

                // Kiểm tra Assessment deadline nếu quiz thuộc về assessment
                if (quiz.AssessmentId > 0)
                {
                    var assessment = await _assessmentRepository.GetAssessmentById(quiz.AssessmentId);
                    if (assessment != null)
                    {
                        // Check enrollment: User phải enroll vào course để làm quiz
                        var module = await _moduleRepository.GetModuleWithCourseAsync(assessment.ModuleId);
                        if (module == null)
                        {
                            response.Success = false;
                            response.Message = "Không tìm thấy Module";
                            response.StatusCode = 404;
                            return response;
                        }

                        var courseId = module.Lesson?.CourseId;
                        if (!courseId.HasValue)
                        {
                            response.Success = false;
                            response.Message = "Không tìm thấy khóa học";
                            response.StatusCode = 404;
                            return response;
                        }

                        var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, userId);
                        if (!isEnrolled)
                        {
                            response.Success = false;
                            response.StatusCode = 403;
                            response.Message = "Bạn cần đăng ký khóa học để làm Quiz này";
                            _logger.LogWarning("User {UserId} attempted to start quiz {QuizId} without enrollment", 
                                userId, quizId);
                            return response;
                        }

                        // Check deadline
                        if (assessment.DueAt.HasValue)
                        {
                            if (DateTime.UtcNow > assessment.DueAt.Value)
                            {
                                response.Success = false;
                                response.Message = "Assessment đã quá hạn nộp bài";
                                response.StatusCode = 403;
                                return response;
                            }
                        }
                    }
                }

                // 2. Kiểm tra user tồn tại
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User không tồn tại";
                    response.StatusCode = 404;
                    return response;
                }

                // 2.1 Kiểm tra xem user có bài nào khác đang làm dở không (prevent concurrent quizzes)
                var existingActiveAttempt = await _quizAttemptRepository.GetAnyActiveAttemptByUserIdAsync(userId);
                if (existingActiveAttempt != null)
                {
                    // Check active attempt expiration
                    var existingQuiz = existingActiveAttempt.Quiz;
                    bool isExpired = false;

                    if (existingQuiz != null && existingQuiz.Duration.HasValue)
                    {
                        var endTime = existingActiveAttempt.StartedAt.AddMinutes(existingQuiz.Duration.Value);
                        if (DateTime.UtcNow >= endTime)
                        {
                            // Đã hết hạn -> Auto submit
                            existingActiveAttempt.Status = QuizAttemptStatus.Submitted;
                            existingActiveAttempt.SubmittedAt = endTime;
                            existingActiveAttempt.TimeSpentSeconds = existingQuiz.Duration.Value * 60;
                            await _quizAttemptRepository.UpdateQuizAttemptAsync(existingActiveAttempt);
                            
                            // Log info
                            _logger.LogInformation("Auto-submitted expired attempt {AttemptId} while starting new quiz {NewQuizId}", 
                                existingActiveAttempt.AttemptId, quizId);
                            
                            isExpired = true;
                        }
                    }

                    // Nếu chưa hết hạn -> Block
                    if (!isExpired)
                    {
                        response.Success = false;
                        response.Message = $"Bạn đang làm dở bài quiz '{existingQuiz?.Title ?? "Unknown"}'. Vui lòng hoàn thành hoặc nộp bài trước khi bắt đầu bài mới.";
                        response.StatusCode = 409; // Conflict
                        return response;
                    }
                }

                // 3. Kiểm tra còn lượt làm bài không
                var quizAttempts = await _quizAttemptRepository.GetByUserAndQuizAsync(userId, quizId);

                // 3.1. Kiểm tra MaxAttempts nếu có quy định
                if (quiz.MaxAttempts.HasValue && quiz.MaxAttempts.Value > 0)
                {
                    // Đếm số attempts đã submit (Submitted, Graded) - không tính InProgress vì có thể do disconnect
                    int submittedAttemptsCount = quizAttempts.Count(a =>
                        a.Status == QuizAttemptStatus.Submitted ||
                        a.Status == QuizAttemptStatus.Graded ||
                        a.Status == QuizAttemptStatus.TimeExpired);

                    if (submittedAttemptsCount >= quiz.MaxAttempts.Value)
                    {
                        response.Success = false;
                        response.Message = $"Bạn đã hết lượt làm bài. Số lần làm tối đa: {quiz.MaxAttempts.Value}. Bạn đã làm {submittedAttemptsCount} lần.";
                        response.StatusCode = 403;
                        return response;
                    }
                }

                //  Lấy full quiz để shuffle
                var quizDetails = await _quizRepository.GetFullQuizAsync(quizId);
                if (quizDetails == null)
                {
                    response.Success = false;
                    response.Message = "Quiz details not found";
                    response.StatusCode = 404;
                    return response;
                }

                //  Tính AttemptNumber cho lần làm bài mới
                int newAttemptNumber = quizAttempts.Count != 0
                    ? quizAttempts.Max(a => a.AttemptNumber) + 1
                    : 1;

                //  Tạo QuizAttempt mới
                var newAttempt = new QuizAttempt
                {
                    QuizId = quizId,
                    UserId = userId,
                    AttemptNumber = newAttemptNumber,
                    StartedAt = DateTime.UtcNow,
                    Status = QuizAttemptStatus.InProgress,
                    TimeSpentSeconds = 0,
                    TotalScore = 0,
                    ScoresJson = null
                };

                await _quizAttemptRepository.AddQuizAttemptAsync(newAttempt);

                //  Áp dụng shuffle nếu có bật trong quiz
                bool shuffleQuestions = quizDetails.ShuffleQuestions == true;
                bool shuffleAnswers = quizDetails.ShuffleAnswers == true;

                var shuffledSections = _quizAttemptMapper.ShuffleQuizForAttempt(quizDetails, newAttempt.AttemptId);

                var attemptDto = _mapper.Map<QuizAttemptWithQuestionsDto>(newAttempt);
                attemptDto.QuizSections = shuffledSections;
                response.Success = true;
                response.Data = attemptDto;
                response.StatusCode = 201;
                response.Message = "Bắt đầu làm quiz thành công";

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
                return response;
            }
        }

        // Update câu trả lời và tính điểm ngay lập tức (real-time scoring)
        // Khi user làm câu nào, sẽ update answer và chấm điểm luôn
        // Nếu làm đúng rồi sửa lại sai thì điểm sẽ từ có điểm thành 0 điểm
        public async Task<ServiceResponse<decimal>> UpdateAnswerAndScoreAsync(int attemptId, UpdateAnswerRequestDto request, int userId)
        {
            var response = new ServiceResponse<decimal>();

            try
            {
                // 1. Lấy attempt và check ownership
                var attempt = await _quizAttemptRepository.GetByIdAndUserIdAsync(attemptId, userId);
                if (attempt == null)
                {
                    _logger.LogWarning("User {UserId} attempted to update answer for attempt {AttemptId} that doesn't exist or doesn't belong to them", 
                        userId, attemptId);
                    response.Success = false;
                    response.Message = "Attempt not found or you don't have permission";
                    response.StatusCode = 404;
                    return response;
                }

                if (attempt.Status != QuizAttemptStatus.InProgress)
                {
                    response.Success = false;
                    response.Message = "Attempt is not in progress";
                    response.StatusCode = 400;
                    return response;
                }

                // 2. Lấy question
                var question = await _questionRepository.GetQuestionByIdAsync(request.QuestionId);
                if (question == null)
                {
                    response.Success = false;
                    response.Message = "Question not found";
                    response.StatusCode = 404;
                    return response;
                }

                // 3. Tìm scoring strategy
                var strategy = _scoringStrategies.FirstOrDefault(s => s.Type == question.Type);
                if (strategy == null)
                {
                    response.Success = false;
                    response.Message = "Scoring strategy not found for question type";
                    response.StatusCode = 500;
                    return response;
                }

                // 4. Tính score mới dựa trên answer mới (real-time scoring)
                // Strategy sẽ tự normalize answer để đảm bảo an toàn
                // Nếu làm đúng rồi sửa lại sai thì điểm sẽ từ có điểm thành 0 điểm
                decimal newScore = strategy.CalculateScore(question, request.UserAnswer);

                // 5. Update AnswersJson (lưu raw answer - Strategy đã tự normalize khi tính điểm)
                var answers = AnswerNormalizer.DeserializeAnswersJson(attempt.AnswersJson);
                answers[request.QuestionId] = request.UserAnswer!;
                attempt.AnswersJson = System.Text.Json.JsonSerializer.Serialize(answers);

                // 6. Update ScoresJson (lưu score mới cho câu này)
                // Nếu câu này đã có điểm trước đó, sẽ bị ghi đè bằng điểm mới
                var scores = AnswerNormalizer.DeserializeScoresJson(attempt.ScoresJson);
                scores[request.QuestionId] = newScore;
                attempt.ScoresJson = System.Text.Json.JsonSerializer.Serialize(scores);

                // 7. Tính lại TotalScore (tổng tất cả scores)
                attempt.TotalScore = scores.Values.Sum();

                // 8. Lưu attempt
                await _quizAttemptRepository.UpdateQuizAttemptAsync(attempt);

                response.Success = true;
                response.Data = newScore;
                response.StatusCode = 200;
                response.Message = "Answer and score updated successfully";

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
                return response;
            }
        }

        public async Task<ServiceResponse<QuizAttemptResultDto>> SubmitQuizAttemptAsync(int attemptId, int userId)
        {
            var response = new ServiceResponse<QuizAttemptResultDto>();

            try
            {
                // 1. Lấy attempt và check ownership
                var attempt = await _quizAttemptRepository.GetByIdAndUserIdAsync(attemptId, userId);
                if (attempt == null)
                {
                    _logger.LogWarning("User {UserId} attempted to submit attempt {AttemptId} that doesn't exist or doesn't belong to them", 
                        userId, attemptId);
                    response.Success = false;
                    response.Message = "Attempt not found or you don't have permission";
                    response.StatusCode = 404;
                    return response;
                }

                if (attempt.Status != QuizAttemptStatus.InProgress)
                {
                    response.Success = false;
                    response.Message = "Attempt is not in progress";
                    response.StatusCode = 400;
                    return response;
                }

                // 2. Cập nhật trạng thái và thời gian submit
                attempt.Status = QuizAttemptStatus.Submitted;
                attempt.SubmittedAt = DateTime.UtcNow;
                attempt.TimeSpentSeconds = (int)(attempt.SubmittedAt.Value - attempt.StartedAt).TotalSeconds;

                await _quizAttemptRepository.UpdateQuizAttemptAsync(attempt);

                // Kiểm tra xem giáo viên cho hs xem nhung thonm tin j sau khi submit  va cho hoc sinh do xem nhung thong tin do
                var quiz = await _quizRepository.GetQuizByIdAsync(attempt.QuizId);
                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = "Quiz not found";
                    response.StatusCode = 404;
                    return response;
                }

                // ✅ Mark module as completed after quiz submission
                var assessment = await _assessmentRepository.GetAssessmentById(quiz.AssessmentId);
                if (assessment?.ModuleId != null)
                {
                    await _moduleProgressService.CompleteModuleAsync(attempt.UserId, assessment.ModuleId);
                }

                // Tạo result object
                var result = new QuizAttemptResultDto
                {
                    AttemptId = attempt.AttemptId,
                    SubmittedAt = attempt.SubmittedAt.Value,
                    TimeSpentSeconds = attempt.TimeSpentSeconds
                };

                // Tính toán điểm số và thông tin khác dựa trên quyền teacher cho phép
                if (quiz.ShowScoreImmediately == true)
                {
                    result.TotalScore = attempt.TotalScore;

                    // Tính percentage dựa trên số câu đúng / tổng số câu hỏi
                    int totalQuestions = quiz.TotalQuestions;
                    int correctQuestions = 0;

                    if (!string.IsNullOrEmpty(attempt.ScoresJson))
                    {
                        var scores = AnswerNormalizer.DeserializeScoresJson(attempt.ScoresJson);
                        correctQuestions = scores.Count(s => s.Value > 0); // Đếm câu có điểm > 0 (đúng)
                    }

                    result.Percentage = totalQuestions > 0 ? (decimal)((double)correctQuestions / totalQuestions) * 100 : 0;

                    // Kiểm tra pass/fail
                    result.IsPassed = quiz.PassingScore.HasValue ? attempt.TotalScore >= quiz.PassingScore.Value : false;

                    // Parse ScoresJson để điền ScoresByQuestion
                    if (!string.IsNullOrEmpty(attempt.ScoresJson))
                    {
                        var scores = AnswerNormalizer.DeserializeScoresJson(attempt.ScoresJson);
                        result.ScoresByQuestion = scores;
                    }
                }

                // Lấy đáp án đúng nếu teacher cho phép
                if (quiz.ShowAnswersAfterSubmit == true)
                {
                    // Build chi tiết câu hỏi như Teacher review
                    result.Questions = QuizReviewBuilder.BuildQuestionReviewList(quiz, attempt);
                }

                // Tạo notification nộp quiz thành công
                try
                {
                    var notification = new Notification
                    {
                        UserId = attempt.UserId,
                        Title = " Nộp bài quiz thành công",
                        Message = quiz.ShowScoreImmediately == true 
                            ? $"Bạn đã nộp bài quiz '{quiz.Title}' thành công. Điểm: {attempt.TotalScore}/{quiz.TotalQuestions * 10}" 
                            : $"Bạn đã nộp bài quiz '{quiz.Title}' thành công. Giáo viên sẽ công bố kết quả sau.",
                        Type = NotificationType.AssessmentGraded,
                        RelatedEntityType = "Quiz",
                        RelatedEntityId = quiz.QuizId,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _notificationRepository.AddAsync(notification);
                }
                catch (Exception notifEx)
                {
                    // Không làm ảnh hưởng đến việc submit quiz
                    _logger.LogWarning(notifEx, "Failed to create quiz notification for attempt {AttemptId}", attempt.AttemptId);
                }

                response.Success = true;
                response.Data = result;
                response.StatusCode = 200;
                response.Message = "Quiz submitted successfully";

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> CheckAndAutoSubmitExpiredAttemptsAsync()
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // BATCH PROCESSING: Load attempts từng batch để tránh memory overflow
                const int BATCH_SIZE = 200; // 200 attempts/batch
                int totalSubmitted = 0;
                int skip = 0;

                // Đếm tổng số attempts cần xử lý
                int totalCount = await _quizAttemptRepository.CountInProgressAttemptsAsync();
                if (totalCount == 0)
                {
                    response.Success = true;
                    response.Data = true;
                    response.Message = "No attempts to auto-submit";
                    return response;
                }

                _logger.LogInformation("Auto-submit: Processing {TotalCount} InProgress attempts", totalCount);

                while (skip < totalCount)
                {
                    // Lấy batch từ DB (chỉ 200 records vào memory)
                    var batch = await _quizAttemptRepository.GetInProgressAttemptsBatchAsync(skip, BATCH_SIZE);

                    if (batch.Count == 0)
                    {
                        break;
                    }

                    try
                    {
                        var now = DateTime.UtcNow;
                        int batchSubmitted = 0;

                        foreach (var attempt in batch)
                        {
                            try
                            {
                                // Quiz đã được Include trong GetInProgressAttemptsAsync → không cần query lại
                                var quiz = attempt.Quiz;
                                if (quiz == null || !quiz.Duration.HasValue) continue;

                                // Tính thời gian kết thúc
                                var endTime = attempt.StartedAt.AddMinutes(quiz.Duration.Value);

                                // Nếu đã hết thời gian
                                if (now >= endTime)
                                {
                                    // Auto-submit
                                    attempt.Status = QuizAttemptStatus.Submitted;
                                    attempt.SubmittedAt = endTime; // Thời gian hết hạn
                                    attempt.TimeSpentSeconds = quiz.Duration.Value * 60; // Thời gian tối đa

                                    await _quizAttemptRepository.UpdateQuizAttemptAsync(attempt);

                                    // Mark module as completed (giống manual submit)
                                    try
                                    {
                                        var assessment = await _assessmentRepository.GetAssessmentById(quiz.AssessmentId);
                                        if (assessment?.ModuleId != null)
                                        {
                                            await _moduleProgressService.CompleteModuleAsync(attempt.UserId, assessment.ModuleId);
                                        }
                                    }
                                    catch (Exception moduleEx)
                                    {
                                        _logger.LogError(moduleEx, "Failed to mark module completed for attempt {AttemptId}", attempt.AttemptId);
                                    }

                                    // Tạo notification cho user
                                    try
                                    {
                                        var notification = new Notification
                                        {
                                            UserId = attempt.UserId,
                                            Title = "⏰ Quiz đã hết thời gian",
                                            Message = $"Bài quiz '{quiz.Title}' của bạn đã được tự động nộp do hết thời gian làm bài.",
                                            Type = NotificationType.AssessmentGraded,
                                            IsRead = false,
                                            CreatedAt = DateTime.UtcNow
                                        };
                                        await _notificationRepository.AddAsync(notification);
                                    }
                                    catch (Exception notifEx)
                                    {
                                        _logger.LogError(notifEx, "Failed to create notification for attempt {AttemptId}", attempt.AttemptId);
                                    }

                                    batchSubmitted++;
                                }
                            }
                            catch (Exception attemptEx)
                            {
                                _logger.LogError(attemptEx, "Failed to auto-submit attempt {AttemptId}", attempt.AttemptId);
                                // Continue với attempt tiếp theo
                            }
                        }

                        totalSubmitted += batchSubmitted;

                        if (batchSubmitted > 0)
                        {
                            Console.WriteLine($" Auto-submitted batch: {batchSubmitted} attempts");
                        }
                    }
                    catch (Exception batchEx)
                    {
                        Console.WriteLine($" Error in batch processing: {batchEx.Message}. Continuing...");
                    }

                    skip += BATCH_SIZE;

                    // Throttle để tránh quá tải DB
                    await Task.Delay(100);
                }

                response.Success = true;
                response.Data = true;
                response.Message = $"Auto-submitted {totalSubmitted} expired attempts (processed {totalCount} total)";

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
                return response;
            }
        }

        public async Task<ServiceResponse<QuizAttemptWithQuestionsDto>> ResumeQuizAttemptAsync(int attemptId, int userId)
        {
            var response = new ServiceResponse<QuizAttemptWithQuestionsDto>();

            try
            {
                // 1. Lấy attempt và check ownership
                var attempt = await _quizAttemptRepository.GetByIdAndUserIdAsync(attemptId, userId);
                if (attempt == null)
                {
                    _logger.LogWarning("User {UserId} attempted to resume attempt {AttemptId} that doesn't exist or doesn't belong to them", 
                        userId, attemptId);
                    response.Success = false;
                    response.Message = "Attempt not found or you don't have permission";
                    response.StatusCode = 404;
                    return response;
                }

                // 2. Kiểm tra trạng thái
                if (attempt.Status != QuizAttemptStatus.InProgress)
                {
                    response.Success = false;
                    response.Message = "Attempt is not in progress";
                    response.StatusCode = 400;
                    return response;
                }

                // 3. Kiểm tra thời gian
                var quiz = await _quizRepository.GetQuizByIdAsync(attempt.QuizId);
                if (quiz?.Duration != null)
                {
                    var endTime = attempt.StartedAt.AddMinutes(quiz.Duration.Value);
                    if (DateTime.UtcNow >= endTime)
                    {
                        // Auto-submit nếu đã hết thời gian
                        attempt.Status = QuizAttemptStatus.Submitted;
                        attempt.SubmittedAt = endTime;
                        attempt.TimeSpentSeconds = quiz.Duration.Value * 60;
                        await _quizAttemptRepository.UpdateQuizAttemptAsync(attempt);

                        response.Success = false;
                        response.Message = "Time has expired. Attempt has been auto-submitted.";
                        response.StatusCode = 400;
                        return response;
                    }
                }

                // 4. Lấy lại cấu trúc quiz (với shuffle đã áp dụng)
                var quizDetails = await _quizRepository.GetFullQuizAsync(attempt.QuizId);
                if (quizDetails == null)
                {
                    response.Success = false;
                    response.Message = "Quiz details not found";
                    response.StatusCode = 404;
                    return response;
                }

                // 5. Áp dụng lại shuffle với cùng seed (attemptId)
                var shuffledSections = _quizAttemptMapper.ShuffleQuizForAttempt(quizDetails, attempt.AttemptId);

                // 6. Parse answers từ DB (chỉ load answers, KHÔNG load scores khi đang làm bài)
                var currentAnswers = AnswerNormalizer.DeserializeAnswersJson(attempt.AnswersJson);

                // 7. Populate answers vào questions trong DTO (KHÔNG hiển thị điểm khi InProgress)
                // Lý do: User không nên biết mình làm đúng hay sai khi đang làm bài
                foreach (var section in shuffledSections)
                {
                    foreach (var item in section.Items)
                    {
                        // Flatten structure: Check ItemType instead of using "is"
                        if (item.ItemType == "Group")
                        {
                            foreach (var question in item.Questions ?? new List<QuestionDto>())
                            {
                                if (currentAnswers.TryGetValue(question.QuestionId, out object? value))
                                {
                                    question.IsAnswered = true;
                                    question.UserAnswer = value;

                                    // Normalize answer theo QuestionType để đảm bảo format đúng cho frontend
                                    question.UserAnswer = AnswerNormalizer.NormalizeUserAnswer(
                                        question.UserAnswer,
                                        question.Type
                                    );

                                    // KHÔNG set CurrentScore khi đang làm bài (InProgress)
                                    // Chỉ hiển thị điểm sau khi submit
                                }
                            }
                        }
                        else if (item.ItemType == "Question")
                        {
                            if (item.QuestionId.HasValue && currentAnswers.TryGetValue(item.QuestionId.Value, out object? value))
                            {
                                item.IsAnswered = true;
                                item.UserAnswer = value;

                                // Normalize answer theo QuestionType để đảm bảo format đúng cho frontend
                                item.UserAnswer = AnswerNormalizer.NormalizeUserAnswer(
                                    item.UserAnswer,
                                    item.Type ?? Domain.Enums.QuestionType.MultipleChoice
                                );

                                // KHÔNG set CurrentScore khi đang làm bài (InProgress)
                                // Chỉ hiển thị điểm sau khi submit
                            }
                        }
                    }
                }

            var attemptDto = _mapper.Map<QuizAttemptWithQuestionsDto>(attempt);
                attemptDto.QuizSections = shuffledSections;

                response.Success = true;
                response.Data = attemptDto;
                response.StatusCode = 200;
                response.Message = "Attempt resumed successfully";

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
                return response;
            }
        }

        public async Task<ServiceResponse<ActiveAttemptDto>> CheckActiveAttemptAsync(int quizId, int userId)
        {
            var response = new ServiceResponse<ActiveAttemptDto>();

            try
            {
                // 1. Lấy attempt đang InProgress (nếu có)
                var activeAttempt = await _quizAttemptRepository.GetActiveAttemptAsync(userId, quizId);

                if (activeAttempt == null)
                {
                    // Không có bài đang làm
                    response.Success = true;
                    response.Data = new ActiveAttemptDto
                    {
                        HasActiveAttempt = false
                    };
                    response.StatusCode = 200;
                    response.Message = "No active attempt found";
                    return response;
                }

                // 2. Kiểm tra thời gian (nếu quiz có duration)
                var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
                DateTime? endTime = null;
                int? timeRemainingSeconds = null;

                if (quiz?.Duration != null)
                {
                    endTime = activeAttempt.StartedAt.AddMinutes(quiz.Duration.Value);
                    var timeRemaining = endTime.Value - DateTime.UtcNow;

                    if (timeRemaining.TotalSeconds <= 0)
                    {
                        // Hết giờ rồi → Auto-submit
                        activeAttempt.Status = QuizAttemptStatus.Submitted;
                        activeAttempt.SubmittedAt = endTime;
                        activeAttempt.TimeSpentSeconds = quiz.Duration.Value * 60;
                        await _quizAttemptRepository.UpdateQuizAttemptAsync(activeAttempt);

                        _logger.LogInformation("Auto-submitted expired attempt {AttemptId} for user {UserId}",
                            activeAttempt.AttemptId, userId);

                        // Trả về không có active attempt
                        response.Success = true;
                        response.Data = new ActiveAttemptDto
                        {
                            HasActiveAttempt = false
                        };
                        response.StatusCode = 200;
                        response.Message = "Previous attempt was auto-submitted due to timeout";
                        return response;
                    }

                    timeRemainingSeconds = (int)timeRemaining.TotalSeconds;
                }

                // 3. Có bài đang làm và còn thời gian
                response.Success = true;
                response.Data = new ActiveAttemptDto
                {
                    HasActiveAttempt = true,
                    AttemptId = activeAttempt.AttemptId,
                    QuizId = activeAttempt.QuizId,
                    QuizTitle = quiz?.Title,
                    StartedAt = activeAttempt.StartedAt,
                    EndTime = endTime,
                    TimeRemainingSeconds = timeRemainingSeconds
                };
                response.StatusCode = 200;
                response.Message = "Active attempt found";

                _logger.LogInformation("User {UserId} has active attempt {AttemptId} for quiz {QuizId}",
                    userId, activeAttempt.AttemptId, quizId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking active attempt for quiz {QuizId} and user {UserId}",
                    quizId, userId);
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
                return response;
            }
        }

        public async Task<ServiceResponse<ActiveAttemptDto>> GetAnyActiveAttemptAsync(int userId)
        {
            var response = new ServiceResponse<ActiveAttemptDto>();

            try
            {
                // 1. Lấy bất kỳ attempt nào đang InProgress của user
                var activeAttempt = await _quizAttemptRepository.GetAnyActiveAttemptByUserIdAsync(userId);

                if (activeAttempt == null)
                {
                    response.Success = true;
                    response.Data = new ActiveAttemptDto { HasActiveAttempt = false };
                    response.StatusCode = 200;
                    response.Message = "No active attempt found";
                    return response;
                }

                // 2. Kiểm tra thời gian (nếu quiz có duration)
                var quiz = activeAttempt.Quiz;
                DateTime? endTime = null;
                int? timeRemainingSeconds = null;

                if (quiz != null && quiz.Duration.HasValue)
                {
                    endTime = activeAttempt.StartedAt.AddMinutes(quiz.Duration.Value);
                    var timeRemaining = endTime.Value - DateTime.UtcNow;

                    if (timeRemaining.TotalSeconds <= 0)
                    {
                        // Hết giờ rồi -> Auto-submit
                        activeAttempt.Status = QuizAttemptStatus.Submitted;
                        activeAttempt.SubmittedAt = endTime;
                        activeAttempt.TimeSpentSeconds = quiz.Duration.Value * 60;
                        await _quizAttemptRepository.UpdateQuizAttemptAsync(activeAttempt);

                        _logger.LogInformation("Auto-submitted expired attempt {AttemptId} for user {UserId} (global check)",
                            activeAttempt.AttemptId, userId);

                        response.Success = true;
                        response.Data = new ActiveAttemptDto { HasActiveAttempt = false };
                        response.StatusCode = 200;
                        response.Message = "Previous attempt was auto-submitted due to timeout";
                        return response;
                    }

                    timeRemainingSeconds = (int)timeRemaining.TotalSeconds;
                }

                // 3. Có bài đang làm
                response.Success = true;
                response.Data = new ActiveAttemptDto
                {
                    HasActiveAttempt = true,
                    AttemptId = activeAttempt.AttemptId,
                    QuizId = activeAttempt.QuizId,
                    QuizTitle = quiz?.Title,
                    StartedAt = activeAttempt.StartedAt,
                    EndTime = endTime,
                    TimeRemainingSeconds = timeRemainingSeconds
                };
                response.StatusCode = 200;
                response.Message = "Active attempt found";

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting any active attempt for user {UserId}", userId);
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
                return response;
            }
        }
    }
}
