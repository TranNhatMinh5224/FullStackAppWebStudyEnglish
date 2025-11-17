using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Strategies;
using AutoMapper;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Common.Helpers;
using System.Text.Json;
using System.Linq;

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

        // Helper method để deserialize Dictionary<int, decimal> an toàn
        private Dictionary<int, decimal> DeserializeScoresJson(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<int, decimal>();

            try
            {
                // Deserialize về Dictionary<string, decimal> trước (vì JSON keys luôn là string)
                var stringDict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
                if (stringDict == null)
                    return new Dictionary<int, decimal>();

                // Convert sang Dictionary<int, decimal>
                return stringDict.ToDictionary(
                    kvp => int.Parse(kvp.Key),
                    kvp => kvp.Value
                );
            }
            catch
            {
                return new Dictionary<int, decimal>();
            }
        }

        // Helper method để deserialize Dictionary<int, object> an toàn
        private Dictionary<int, object> DeserializeAnswersJson(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<int, object>();

            try
            {
                // Deserialize về Dictionary<string, JsonElement> trước
                var stringDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (stringDict == null)
                    return new Dictionary<int, object>();

                // Convert sang Dictionary<int, object>
                var result = new Dictionary<int, object>();
                foreach (var kvp in stringDict)
                {
                    var key = int.Parse(kvp.Key);
                    // Convert JsonElement về object thực sự
                    result[key] = ConvertJsonElementToObject(kvp.Value);
                }
                return result;
            }
            catch
            {
                return new Dictionary<int, object>();
            }
        }

        // Helper method để normalize userAnswer (convert string sang int, xử lý JsonElement, etc.)
        private object? NormalizeUserAnswer(object? userAnswer, QuestionType questionType)
        {
            if (userAnswer == null)
                return null;

            // Nếu là JsonElement, convert về object trước
            if (userAnswer is JsonElement jsonElement)
            {
                userAnswer = ConvertJsonElementToObject(jsonElement);
            }

            // Xử lý theo từng loại câu hỏi
            switch (questionType)
            {
                case QuestionType.MultipleChoice:
                case QuestionType.TrueFalse:
                    // Cần int (optionId)
                    if (userAnswer is int intValue)
                        return intValue;
                    if (userAnswer is string stringValue && int.TryParse(stringValue, out int parsedInt))
                        return parsedInt;
                    if (userAnswer is long longValue)
                        return (int)longValue;
                    if (userAnswer is decimal decimalValue)
                        return (int)decimalValue;
                    // Nếu không convert được, trả về nguyên bản
                    return userAnswer;

                case QuestionType.MultipleAnswers:
                    // Cần List<int>
                    if (userAnswer is List<int> intList)
                        return intList;
                    if (userAnswer is List<object> objectList)
                    {
                        var convertedList = new List<int>();
                        foreach (var item in objectList)
                        {
                            if (item is int i)
                                convertedList.Add(i);
                            else if (item is string s && int.TryParse(s, out int parsed))
                                convertedList.Add(parsed);
                            else if (item is JsonElement je && je.TryGetInt32(out int jeInt))
                                convertedList.Add(jeInt);
                        }
                        return convertedList;
                    }
                    // Nếu là array từ JSON
                    if (userAnswer is JsonElement jeArray && jeArray.ValueKind == JsonValueKind.Array)
                    {
                        var list = new List<int>();
                        foreach (var item in jeArray.EnumerateArray())
                        {
                            if (item.TryGetInt32(out int itemInt))
                                list.Add(itemInt);
                        }
                        return list;
                    }
                    return userAnswer;

                case QuestionType.FillBlank:
                    // Cần string
                    if (userAnswer is string str)
                        return str;
                    return userAnswer?.ToString() ?? string.Empty;

                default:
                    return userAnswer;
            }
        }

        // Helper method để convert JsonElement về object thực sự
        private object ConvertJsonElementToObject(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString() ?? string.Empty;
                
                case JsonValueKind.Number:
                    // Thử parse thành int trước, nếu không được thì decimal
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    if (element.TryGetDecimal(out decimal decimalValue))
                        return decimalValue;
                    return element.GetDouble();
                
                case JsonValueKind.True:
                    return true;
                
                case JsonValueKind.False:
                    return false;
                
                case JsonValueKind.Array:
                    // Kiểm tra xem có phải là array of numbers không (List<int>)
                    var arrayList = element.EnumerateArray().ToList();
                    if (arrayList.Count == 0)
                    {
                        return new List<int>();
                    }
                    
                    var firstElement = arrayList[0];
                    if (firstElement.ValueKind == JsonValueKind.Number)
                    {
                        var intList = new List<int>();
                        foreach (var item in arrayList)
                        {
                            if (item.TryGetInt32(out int itemValue))
                                intList.Add(itemValue);
                        }
                        return intList;
                    }
                    // Nếu là array of strings
                    if (firstElement.ValueKind == JsonValueKind.String)
                    {
                        var stringList = new List<string>();
                        foreach (var item in arrayList)
                        {
                            stringList.Add(item.GetString() ?? string.Empty);
                        }
                        return stringList;
                    }
                    // Fallback: return as JsonElement
                    return element;
                
                case JsonValueKind.Object:
                    // Nested object, return as JsonElement hoặc có thể deserialize thêm
                    return element;
                
                case JsonValueKind.Null:
                default:
                    return null!;
            }
        }

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

        // Update câu trả lời và tính điểm ngay lập tức (real-time scoring)
        // Khi user làm câu nào, sẽ update answer và chấm điểm luôn
        // Nếu làm đúng rồi sửa lại sai thì điểm sẽ từ có điểm thành 0 điểm
        public async Task<ServiceResponse<decimal>> UpdateAnswerAndScoreAsync(int attemptId, UpdateAnswerRequestDto request)
        {
            var response = new ServiceResponse<decimal>();

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

                // 4. Normalize userAnswer (convert string sang int nếu cần, xử lý JsonElement, etc.)
                object? normalizedAnswer = NormalizeUserAnswer(request.UserAnswer, question.Type);

                // 5. Tính score mới dựa trên answer mới (real-time scoring)
                // Nếu làm đúng rồi sửa lại sai thì điểm sẽ từ có điểm thành 0 điểm
                decimal newScore = strategy.CalculateScore(question, normalizedAnswer);

                // 6. Update AnswersJson (lưu answer mới - lưu normalized answer)
                var answers = DeserializeAnswersJson(attempt.AnswersJson);
                answers[request.QuestionId] = normalizedAnswer!;
                attempt.AnswersJson = System.Text.Json.JsonSerializer.Serialize(answers);

                // 7. Update ScoresJson (lưu score mới cho câu này)
                // Nếu câu này đã có điểm trước đó, sẽ bị ghi đè bằng điểm mới
                var scores = DeserializeScoresJson(attempt.ScoresJson);
                scores[request.QuestionId] = newScore;
                attempt.ScoresJson = System.Text.Json.JsonSerializer.Serialize(scores);

                // 8. Tính lại TotalScore (tổng tất cả scores)
                attempt.TotalScore = scores.Values.Sum();

                // 9. Lưu attempt
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
                        var scores = DeserializeScoresJson(attempt.ScoresJson);
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
                var currentScores = DeserializeScoresJson(attempt.ScoresJson);

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
