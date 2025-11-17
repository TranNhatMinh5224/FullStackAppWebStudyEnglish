using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using AutoMapper;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Common.Helpers;

namespace LearningEnglish.Application.Service
{
    public class QuizAttemptService : IQuizAttemptService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly IMapper _mapper;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEnumerable<IScoringStrategy> _scoringStrategies;
        private readonly IQuestionRepository _questionRepository;

        public QuizAttemptService(
            IQuizRepository quizRepository,
            IQuizAttemptRepository quizAttemptRepository,
            IMapper mapper,
            IAssessmentRepository assessmentRepository,
            IUserRepository userRepository,
            IEnumerable<IScoringStrategy> scoringStrategies,
            IQuestionRepository questionRepository)
        {
            _quizRepository = quizRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _mapper = mapper;
            _assessmentRepository = assessmentRepository;
            _userRepository = userRepository;
            _scoringStrategies = scoringStrategies;
            _questionRepository = questionRepository;

            // Debug: kiểm tra strategies được inject
            Console.WriteLine($"Scoring strategies injected: {scoringStrategies.Count()}");
            foreach (var strategy in scoringStrategies)
            {
                Console.WriteLine($"Strategy: {strategy.GetType().Name} for {strategy.Type}");
            }
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

                if (quiz.Status == QuizStatus.Closed || quiz.Status == QuizStatus.Archived)
                {
                    response.Success = false;
                    response.Message = "Quiz đã đóng hoặc đã lưu trữ";
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

                // 2. Kiểm tra user tồn tại
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User không tồn tại";
                    response.StatusCode = 404;
                    return response;
                }

                // 3. Kiểm tra còn lượt làm bài không
                var quizAttempts = await _quizAttemptRepository.GetByUserAndQuizAsync(userId, quizId);


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
                int newAttemptNumber = quizAttempts.Any()
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

                var shuffledSections = ShuffleQuizForAttempt(quizDetails, newAttempt.AttemptId);

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

        private List<AttemptQuizSectionDto> ShuffleQuizForAttempt(Quiz quiz, int attemptId)
        {
            // Sao chép structure (questions trong groups giữ nguyên)
            var sections = quiz.QuizSections.Select(s => new AttemptQuizSectionDto
            {
                SectionId = s.QuizSectionId,
                Title = s.Title,
                QuizGroups = s.QuizGroups.Select(g => new AttemptQuizGroupDto
                {
                    GroupId = g.QuizGroupId,
                    Name = g.Name,
                    Questions = g.Questions.Select(q => new QuestionDto  // Giữ nguyên thứ tự
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = q.StemText,
                        Type = q.Type,
                        Points = q.Points,
                        IsAnswered = false,
                        CurrentScore = null,
                        Options = q.Options.Select(o => new AnswerOptionDto
                        {
                            OptionId = o.AnswerOptionId,
                            OptionText = o.Text ?? string.Empty
                        }).ToList()
                    }).ToList()
                }).ToList(),
                Questions = s.Questions?.Select(q => new QuestionDto  // Sẽ shuffle
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.StemText,
                    Type = q.Type,
                    Points = q.Points,
                    IsAnswered = false,
                    CurrentScore = null,
                    Options = q.Options.Select(o => new AnswerOptionDto
                    {
                        OptionId = o.AnswerOptionId,
                        OptionText = o.Text ?? string.Empty
                    }).ToList()
                }).ToList() ?? new List<QuestionDto>()
            }).ToList();

            // Shuffle chỉ standalone questions
            if (quiz.ShuffleQuestions.GetValueOrDefault(false))
                QuizShuffleHelper.ShuffleStandaloneQuestions(sections, attemptId);

            // Shuffle answers
            if (quiz.ShuffleAnswers.GetValueOrDefault(false))
            {
                foreach (var section in sections)
                {
                    foreach (var group in section.QuizGroups)
                    {
                        foreach (var question in group.Questions)
                        {
                            QuizShuffleHelper.ShuffleAnswers(question.Options, attemptId);
                        }
                    }
                    foreach (var question in section.Questions)
                    {
                        QuizShuffleHelper.ShuffleAnswers(question.Options, attemptId);
                    }
                }
            }

            return sections;
        }

        public async Task<ServiceResponse<decimal>> UpdateScoreAsync(int quizId  , UpdateScoreRequestDto request)
        {
            var response = new ServiceResponse<decimal>();

            try
            {
                // 1. Lấy attempt
                var attempt = await _quizAttemptRepository.GetByIdAsync(request.AttemptId);
                if (attempt == null || attempt.Status != QuizAttemptStatus.InProgress)
                {
                    response.Success = false;
                    response.Message = "Attempt not found or not in progress";
                    response.StatusCode = 404;
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

                // 3. Tìm strategy phù hợp
                var strategy = _scoringStrategies.FirstOrDefault(s => s.Type == question.Type);
                if (strategy == null)
                {
                    response.Success = false;
                    response.Message = "Scoring strategy not found for question type";
                    response.StatusCode = 500;
                    return response;
                }

                // 4. Tính điểm
                decimal score = strategy.CalculateScore(question, request.UserAnswer);

                // 5. Cập nhật ScoresJson
                var scores = string.IsNullOrEmpty(attempt.ScoresJson)
                    ? new Dictionary<int, decimal>()
                    : System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, decimal>>(attempt.ScoresJson)!;

                scores[request.QuestionId] = score;

                attempt.ScoresJson = System.Text.Json.JsonSerializer.Serialize(scores);

                // 6. Cập nhật TotalScore
                attempt.TotalScore = scores.Values.Sum();

                // 7. Lưu
                await _quizAttemptRepository.UpdateQuizAttemptAsync(attempt);

                response.Success = true;
                response.Data = score;
                response.StatusCode = 200;
                response.Message = "Score updated successfully";

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

        public async Task<ServiceResponse<decimal>> UpdateAnswerAndScoreAsync(int attemptId, UpdateAnswerRequestDto request)
        {
            try
            {
                // 1. Lấy attempt
                var attempt = await _quizAttemptRepository.GetByIdAsync(attemptId);
                if (attempt == null || attempt.Status != QuizAttemptStatus.InProgress)
                    return new ServiceResponse<decimal> { Success = false, Message = "Attempt not found or not in progress" };

                // 2. Lấy question
                var question = await _questionRepository.GetQuestionByIdAsync(request.QuestionId);
                if (question == null)
                    return new ServiceResponse<decimal> { Success = false, Message = "Question not found" };

                // 3. Tìm scoring strategy
                var strategy = _scoringStrategies.FirstOrDefault(s => s.Type == question.Type);
                if (strategy == null)
                    return new ServiceResponse<decimal> { Success = false, Message = "Scoring strategy not found" };

                // 4. Tính score mới dựa trên answer mới
                decimal newScore = strategy.CalculateScore(question, request.UserAnswer);

                // 5. Update AnswersJson (lưu answer mới)
                var answers = string.IsNullOrEmpty(attempt.AnswersJson)
                    ? new Dictionary<int, object>()
                    : System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, object>>(attempt.AnswersJson)!;
                
                answers[request.QuestionId] = request.UserAnswer!;

                attempt.AnswersJson = System.Text.Json.JsonSerializer.Serialize(answers);

                // 6. Update ScoresJson (lưu score mới cho câu này)
                var scores = string.IsNullOrEmpty(attempt.ScoresJson)
                    ? new Dictionary<int, decimal>()
                    : System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, decimal>>(attempt.ScoresJson)!;
                scores[request.QuestionId] = newScore;
                attempt.ScoresJson = System.Text.Json.JsonSerializer.Serialize(scores);

                // 7. Tính lại TotalScore (tổng tất cả scores)
                attempt.TotalScore = scores.Values.Sum();

                // 8. Lưu attempt
                await _quizAttemptRepository.UpdateQuizAttemptAsync(attempt);

                return new ServiceResponse<decimal> { Success = true, Data = newScore, Message = "Answer and score updated" };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<decimal> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponse<QuizAttemptResultDto>> SubmitQuizAttemptAsync(int attemptId)
        {
            var response = new ServiceResponse<QuizAttemptResultDto>();

            try
            {
                // 1. Lấy attempt
                var attempt = await _quizAttemptRepository.GetByIdAsync(attemptId);
                if (attempt == null || attempt.Status != QuizAttemptStatus.InProgress)
                {
                    response.Success = false;
                    response.Message = "Attempt not found or not in progress";
                    response.StatusCode = 404;
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
                    
                    // Tính percentage (điểm hiện tại / điểm tối đa có thể)
                    var maxPossibleScore = quiz.TotalPossibleScore;
                    result.Percentage = maxPossibleScore > 0 ? (attempt.TotalScore / maxPossibleScore) * 100 : 0;
                    
                    // Kiểm tra pass/fail
                    result.IsPassed = quiz.PassingScore.HasValue ? attempt.TotalScore >= quiz.PassingScore.Value : false;
                    
                    // Parse ScoresJson để điền ScoresByQuestion
                    if (!string.IsNullOrEmpty(attempt.ScoresJson))
                    {
                        var scores = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, decimal>>(attempt.ScoresJson)!;
                        result.ScoresByQuestion = scores;
                    }
                }

                // Lấy đáp án đúng nếu teacher cho phép
                if (quiz.ShowAnswersAfterSubmit == true)
                {
                    result.CorrectAnswers = await GetCorrectAnswersAsync(attempt.QuizId);
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
                // Lấy tất cả attempts đang InProgress
                var inProgressAttempts = await _quizAttemptRepository.GetInProgressAttemptsAsync();

                int submittedCount = 0;

                foreach (var attempt in inProgressAttempts)
                {
                    // Lấy thông tin quiz để kiểm tra duration
                    var quiz = await _quizRepository.GetQuizByIdAsync(attempt.QuizId);
                    if (quiz == null || !quiz.Duration.HasValue) continue;

                    // Tính thời gian kết thúc
                    var endTime = attempt.StartedAt.AddMinutes(quiz.Duration.Value);
                    var now = DateTime.UtcNow;

                    // Nếu đã hết thời gian
                    if (now >= endTime)
                    {
                        // Auto-submit
                        attempt.Status = QuizAttemptStatus.Submitted;
                        attempt.SubmittedAt = endTime; // Thời gian hết hạn
                        attempt.TimeSpentSeconds = quiz.Duration.Value * 60; // Thời gian tối đa

                        await _quizAttemptRepository.UpdateQuizAttemptAsync(attempt);
                        submittedCount++;
                    }
                }

                response.Success = true;
                response.Data = true;
                response.Message = $"Auto-submitted {submittedCount} expired attempts";

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

        public async Task<ServiceResponse<QuizAttemptWithQuestionsDto>> ResumeQuizAttemptAsync(int attemptId)
        {
            var response = new ServiceResponse<QuizAttemptWithQuestionsDto>();

            try
            {
                // 1. Lấy attempt
                var attempt = await _quizAttemptRepository.GetByIdAsync(attemptId);
                if (attempt == null)
                {
                    response.Success = false;
                    response.Message = "Attempt not found";
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
                var shuffledSections = ShuffleQuizForAttempt(quizDetails, attempt.AttemptId);

                // 6. Parse scores hiện tại để đánh dấu câu đã trả lời
                var currentScores = string.IsNullOrEmpty(attempt.ScoresJson)
                    ? new Dictionary<int, decimal>()
                    : System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, decimal>>(attempt.ScoresJson)!;

                // 7. Mark questions đã trả lời trong DTO
                foreach (var section in shuffledSections)
                {
                    foreach (var group in section.QuizGroups)
                    {
                        foreach (var question in group.Questions)
                        {
                            if (currentScores.ContainsKey(question.QuestionId))
                            {
                                question.IsAnswered = true;
                                question.CurrentScore = currentScores[question.QuestionId];
                            }
                        }
                    }
                    foreach (var question in section.Questions)
                    {
                        if (currentScores.ContainsKey(question.QuestionId))
                        {
                            question.IsAnswered = true;
                            question.CurrentScore = currentScores[question.QuestionId];
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
        
        private async Task<List<CorrectAnswerDto>> GetCorrectAnswersAsync(int quizId)
        {
            var correctAnswers = new List<CorrectAnswerDto>();
            
            // Lấy tất cả câu hỏi của quiz
            var quizDetails = await _quizRepository.GetFullQuizAsync(quizId);
            if (quizDetails == null) return correctAnswers;

            foreach (var section in quizDetails.QuizSections)
            {
                // Questions trong groups
                foreach (var group in section.QuizGroups)
                {
                    foreach (var question in group.Questions)
                    {
                        correctAnswers.Add(CreateCorrectAnswerDto(question));
                    }
                }
                
                if (section.Questions != null)
                {
                    foreach (var question in section.Questions)
                    {
                        correctAnswers.Add(CreateCorrectAnswerDto(question));
                    }
                }
            }
            
            return correctAnswers;
        }

        private CorrectAnswerDto CreateCorrectAnswerDto(Question question)
        {
            var correctOptions = new List<string>();
            
            // Lấy đáp án đúng dựa trên loại câu hỏi
            if (question.Type == QuestionType.MultipleChoice || question.Type == QuestionType.TrueFalse)
            {
                // Multiple choice: lấy text của options có IsCorrect = true
                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                if (correctOption != null)
                {
                    correctOptions.Add(correctOption.Text ?? string.Empty);
                }
            }
            else if (question.Type == QuestionType.MultipleAnswers)
            {
                // Multiple answers: lấy tất cả options có IsCorrect = true
                correctOptions = question.Options
                    .Where(o => o.IsCorrect)
                    .Select(o => o.Text ?? string.Empty)
                    .ToList();
            }
            else if (question.Type == QuestionType.FillBlank || question.Type == QuestionType.Matching || question.Type == QuestionType.Ordering)
            {
                // Parse từ CorrectAnswersJson
                if (!string.IsNullOrEmpty(question.CorrectAnswersJson))
                {
                    try
                    {
                        // Đơn giản hóa: lấy raw JSON hoặc parse theo format
                        correctOptions.Add($"Correct answer data: {question.CorrectAnswersJson}");
                    }
                    catch
                    {
                        correctOptions.Add("Correct answer available");
                    }
                }
            }
            
            return new CorrectAnswerDto
            {
                QuestionId = question.QuestionId,
                QuestionText = question.StemText,
                CorrectOptions = correctOptions
            };
        }
    }
}
