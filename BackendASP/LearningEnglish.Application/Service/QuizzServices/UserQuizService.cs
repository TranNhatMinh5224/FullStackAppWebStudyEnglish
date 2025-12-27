using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using AutoMapper;

namespace LearningEnglish.Application.Service
{
    public class UserQuizService : IUserQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IMapper _mapper;

        public UserQuizService(IQuizRepository quizRepository, IMapper mapper)
        {
            _quizRepository = quizRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<QuizDto>> GetQuizByIdAsync(int quizId, int userId)
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

        public async Task<ServiceResponse<List<QuizDto>>> GetQuizzesByAssessmentIdAsync(int assessmentId, int userId)
        {
            var response = new ServiceResponse<List<QuizDto>>();
            try
            {
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
    }
}
