using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class UserQuizService : IUserQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserQuizService> _logger;

        public UserQuizService(
            IQuizRepository quizRepository,
            IAssessmentRepository assessmentRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<UserQuizService> logger)
        {
            _quizRepository = quizRepository;
            _assessmentRepository = assessmentRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<QuizDto>> GetQuizByIdAsync(int quizId, int userId)
        {
            var response = new ServiceResponse<QuizDto>();
            try
            {
                var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy Quiz";
                    response.StatusCode = 404;
                    return response;
                }

                // Kiểm tra quiz có thuộc Assessment không
                if (quiz.AssessmentId <= 0)
                {
                    response.Success = false;
                    response.Message = "Quiz này không thuộc về Assessment nào";
                    response.StatusCode = 400;
                    return response;
                }

                // Lấy Assessment với Module và Course để check enrollment
                var assessment = await _assessmentRepository.GetAssessmentById(quiz.AssessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy Assessment";
                    response.StatusCode = 404;
                    return response;
                }

                // Lấy Module với Course để check enrollment
                var module = await _moduleRepository.GetModuleWithCourseAsync(assessment.ModuleId);
                if (module == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy Module";
                    response.StatusCode = 404;
                    return response;
                }

                // Check enrollment
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
                    response.Message = "Bạn cần đăng ký khóa học để xem Quiz này";
                    _logger.LogWarning("User {UserId} attempted to access quiz {QuizId} without enrollment", 
                        userId, quizId);
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
                _logger.LogError(ex, "Error getting quiz {QuizId} for user {UserId}", quizId, userId);
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
                // Lấy Assessment với Module và Course để check enrollment
                var assessment = await _assessmentRepository.GetAssessmentById(assessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy Assessment";
                    response.StatusCode = 404;
                    return response;
                }

                // Lấy Module với Course để check enrollment
                var module = await _moduleRepository.GetModuleWithCourseAsync(assessment.ModuleId);
                if (module == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy Module";
                    response.StatusCode = 404;
                    return response;
                }

                // Check enrollment
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
                    response.Message = "Bạn cần đăng ký khóa học để xem các Quiz";
                    _logger.LogWarning("User {UserId} attempted to list quizzes of assessment {AssessmentId} without enrollment", 
                        userId, assessmentId);
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
                _logger.LogError(ex, "Error getting quizzes for assessment {AssessmentId} and user {UserId}", assessmentId, userId);
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }
            return response;
        }
    }
}
