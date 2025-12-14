using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using AutoMapper;

namespace LearningEnglish.Application.Service
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IMapper _mapper;
        public QuizService(IQuizRepository quizRepository, IAssessmentRepository assessmentRepository, IMapper mapper)
        {
            _quizRepository = quizRepository;
            _assessmentRepository = assessmentRepository;
            _mapper = mapper;
        }
        // Get quiz by id
        public async Task<ServiceResponse<QuizDto>> GetQuizByIdAsync(int quizId)
        {
            var response = new ServiceResponse<QuizDto>();
            try
            {


                var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
                if (quiz == null)
                {
                    return new ServiceResponse<QuizDto>
                    {
                        Success = false,
                        Message = "Quiz not found"
                    };
                }

                var quizDto = _mapper.Map<QuizDto>(quiz);
                response.Data = quizDto;
                response.StatusCode = 200;
                return response;

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }
            return response;



        }
        // Get quizzes by assessment id 
        public async Task<ServiceResponse<List<QuizDto>>> GetQuizzesByAssessmentIdAsync(int assessmentId)
        {
            var response = new ServiceResponse<List<QuizDto>>();
            try
            {
                var quizzes = await _quizRepository.GetQuizzesByAssessmentIdAsync(assessmentId);
                var quizDtos = _mapper.Map<List<QuizDto>>(quizzes);
                response.Data = quizDtos;
                response.StatusCode = 200;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }
            return response;
        }
        // Create new quiz
        public async Task<ServiceResponse<QuizDto>> CreateQuizAsync(QuizCreateDto quizDto, int? teacherId = null)
        {
            var response = new ServiceResponse<QuizDto>();
            try
            {
                // Validation: Check if Assessment exists
                var assessment = await _assessmentRepository.GetAssessmentById(quizDto.AssessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.Message = "Assessment not found";
                    response.StatusCode = 404;
                    return response;
                }

                // 🔒 Check Teacher ownership if teacherId is provided
                if (teacherId.HasValue)
                {
                    if (!await _assessmentRepository.IsTeacherOwnerOfAssessmentAsync(teacherId.Value, quizDto.AssessmentId))
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Teacher không có quyền tạo Quiz cho Assessment này";
                        return response;
                    }
                }

                var quiz = _mapper.Map<Quiz>(quizDto);
                await _quizRepository.AddQuizAsync(quiz);

                // Tính toán TotalPossibleScore sau khi tạo quiz
                var fullQuiz = await _quizRepository.GetFullQuizAsync(quiz.QuizId);
                if (fullQuiz != null)
                {
                    quiz.TotalPossibleScore = CalculateTotalPossibleScore(fullQuiz);
                    await _quizRepository.UpdateQuizAsync(quiz);
                }

                response.Data = _mapper.Map<QuizDto>(quiz);
                response.StatusCode = 201;
                return response;

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }
            return response;
        }
        // Update quiz
        public async Task<ServiceResponse<QuizDto>> UpdateQuizAsync(int quizId, QuizUpdateDto quizDto, int? teacherId = null)
        {
            var response = new ServiceResponse<QuizDto>();
            try
            {
                var existingQuiz = await _quizRepository.GetQuizByIdAsync(quizId);
                if (existingQuiz == null)
                {
                    response.Success = false;
                    response.Message = "Quiz not found";
                    response.StatusCode = 404;
                    return response;
                }

                // 🔒 Check Teacher ownership if teacherId is provided
                if (teacherId.HasValue)
                {
                    if (!await _assessmentRepository.IsTeacherOwnerOfAssessmentAsync(teacherId.Value, existingQuiz.AssessmentId))
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Teacher không có quyền cập nhật Quiz này";
                        return response;
                    }
                }

                _mapper.Map(quizDto, existingQuiz);
                await _quizRepository.UpdateQuizAsync(existingQuiz);

                // Tính toán lại TotalPossibleScore sau khi update
                var fullQuiz = await _quizRepository.GetFullQuizAsync(existingQuiz.QuizId);
                if (fullQuiz != null)
                {
                    existingQuiz.TotalPossibleScore = CalculateTotalPossibleScore(fullQuiz);
                    await _quizRepository.UpdateQuizAsync(existingQuiz);
                }

                response.Data = _mapper.Map<QuizDto>(existingQuiz);
                response.StatusCode = 200;
                return response;

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }
            return response;
        }
        // Delete quiz
        public async Task<ServiceResponse<bool>> DeleteQuizAsync(int quizId, int? teacherId = null)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var existingQuiz = await _quizRepository.GetQuizByIdAsync(quizId);
                if (existingQuiz == null)
                {
                    response.Success = false;
                    response.Message = "Quiz not found";
                    response.StatusCode = 404;
                    response.Data = false;
                    return response;
                }

                // 🔒 Check Teacher ownership if teacherId is provided
                if (teacherId.HasValue)
                {
                    if (!await _assessmentRepository.IsTeacherOwnerOfAssessmentAsync(teacherId.Value, existingQuiz.AssessmentId))
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Teacher không có quyền xóa Quiz này";
                        response.Data = false;
                        return response;
                    }
                }

                await _quizRepository.DeleteQuizAsync(quizId);
                response.Data = true;
                response.StatusCode = 200;
                return response;

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
                response.Data = false;
            }
            return response;

        }


        private static decimal CalculateTotalPossibleScore(Quiz quiz)
        {
            decimal maxScore = 0;
            foreach (var section in quiz.QuizSections)
            {
                // Questions trong groups
                foreach (var group in section.QuizGroups)
                {
                    maxScore += group.Questions.Sum(q => q.Points);
                }
                // Standalone questions
                if (section.Questions != null)
                {
                    maxScore += section.Questions.Sum(q => q.Points);
                }
            }
            return maxScore;
        }
    }
}