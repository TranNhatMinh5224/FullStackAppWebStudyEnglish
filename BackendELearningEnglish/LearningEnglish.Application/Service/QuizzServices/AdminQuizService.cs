using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Domain.Entities;
using AutoMapper;

namespace LearningEnglish.Application.Service
{
    public class AdminQuizService : IAdminQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IMapper _mapper;

        public AdminQuizService(
            IQuizRepository quizRepository, 
            IAssessmentRepository assessmentRepository, 
            IMapper mapper)
        {
            _quizRepository = quizRepository;
            _assessmentRepository = assessmentRepository;
            _mapper = mapper;
        }

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

        public async Task<ServiceResponse<List<QuizDto>>> GetQuizzesByAssessmentIdAsync(int assessmentId)
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

        public async Task<ServiceResponse<QuizDto>> CreateQuizAsync(QuizCreateDto quizDto)
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

                var quiz = _mapper.Map<Quiz>(quizDto);
                // TotalPossibleScore được nhập từ DTO, không tự động tính
                await _quizRepository.AddQuizAsync(quiz);

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

        public async Task<ServiceResponse<QuizDto>> UpdateQuizAsync(int quizId, QuizUpdateDto quizDto)
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

                _mapper.Map(quizDto, existingQuiz);
                // TotalPossibleScore được cập nhật từ DTO, không tự động tính
                await _quizRepository.UpdateQuizAsync(existingQuiz);

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

        public async Task<ServiceResponse<bool>> DeleteQuizAsync(int quizId)
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

    }
}
