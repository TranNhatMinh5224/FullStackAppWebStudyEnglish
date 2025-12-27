using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using AutoMapper;

namespace LearningEnglish.Application.Service
{
    public class TeacherQuizService : ITeacherQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IMapper _mapper;

        public TeacherQuizService(
            IQuizRepository quizRepository, 
            IAssessmentRepository assessmentRepository, 
            IMapper mapper)
        {
            _quizRepository = quizRepository;
            _assessmentRepository = assessmentRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<QuizDto>> GetQuizByIdAsync(int quizId, int teacherId)
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
                        Message = "Quiz not found",
                        StatusCode = 404
                    };
                }

                if (!await _assessmentRepository.IsTeacherOwnerOfAssessmentAsync(teacherId, quiz.AssessmentId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền xem Quiz này";
                    return response;
                }

                var quizDto = _mapper.Map<QuizDto>(quiz);
                response.Data = quizDto;
                response.StatusCode = 200;
                response.Success = true;
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

        public async Task<ServiceResponse<List<QuizDto>>> GetQuizzesByAssessmentIdAsync(int assessmentId, int teacherId)
        {
            var response = new ServiceResponse<List<QuizDto>>();
            try
            {
                if (!await _assessmentRepository.IsTeacherOwnerOfAssessmentAsync(teacherId, assessmentId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền xem Quizzes của Assessment này";
                    return response;
                }

                var quizzes = await _quizRepository.GetQuizzesByAssessmentIdAsync(assessmentId);
                var quizDtos = _mapper.Map<List<QuizDto>>(quizzes);
                response.Data = quizDtos;
                response.StatusCode = 200;
                response.Success = true;
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

        public async Task<ServiceResponse<QuizDto>> CreateQuizAsync(QuizCreateDto quizDto, int teacherId)
        {
            var response = new ServiceResponse<QuizDto>();
            try
            {
                var assessment = await _assessmentRepository.GetAssessmentById(quizDto.AssessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.Message = "Assessment not found";
                    response.StatusCode = 404;
                    return response;
                }

                if (!await _assessmentRepository.IsTeacherOwnerOfAssessmentAsync(teacherId, quizDto.AssessmentId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền tạo Quiz cho Assessment này";
                    return response;
                }

                var quiz = _mapper.Map<Quiz>(quizDto);
                await _quizRepository.AddQuizAsync(quiz);

                var fullQuiz = await _quizRepository.GetFullQuizAsync(quiz.QuizId);
                if (fullQuiz != null)
                {
                    quiz.TotalPossibleScore = CalculateTotalPossibleScore(fullQuiz);
                    await _quizRepository.UpdateQuizAsync(quiz);
                }

                response.Data = _mapper.Map<QuizDto>(quiz);
                response.StatusCode = 201;
                response.Success = true;
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

        public async Task<ServiceResponse<QuizDto>> UpdateQuizAsync(int quizId, QuizUpdateDto quizDto, int teacherId)
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

                if (!await _assessmentRepository.IsTeacherOwnerOfAssessmentAsync(teacherId, existingQuiz.AssessmentId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền cập nhật Quiz này";
                    return response;
                }

                _mapper.Map(quizDto, existingQuiz);
                await _quizRepository.UpdateQuizAsync(existingQuiz);

                var fullQuiz = await _quizRepository.GetFullQuizAsync(existingQuiz.QuizId);
                if (fullQuiz != null)
                {
                    existingQuiz.TotalPossibleScore = CalculateTotalPossibleScore(fullQuiz);
                    await _quizRepository.UpdateQuizAsync(existingQuiz);
                }

                response.Data = _mapper.Map<QuizDto>(existingQuiz);
                response.StatusCode = 200;
                response.Success = true;
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

        public async Task<ServiceResponse<bool>> DeleteQuizAsync(int quizId, int teacherId)
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

                if (!await _assessmentRepository.IsTeacherOwnerOfAssessmentAsync(teacherId, existingQuiz.AssessmentId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền xóa Quiz này";
                    response.Data = false;
                    return response;
                }

                await _quizRepository.DeleteQuizAsync(quizId);
                response.Data = true;
                response.StatusCode = 200;
                response.Success = true;
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
                foreach (var group in section.QuizGroups)
                {
                    maxScore += group.Questions.Sum(q => q.Points);
                }
                if (section.Questions != null)
                {
                    maxScore += section.Questions.Sum(q => q.Points);
                }
            }
            return maxScore;
        }
    }
}
