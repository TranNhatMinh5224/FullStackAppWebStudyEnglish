using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Constants;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Common.Prompts;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
  
    public class UserEssaySubmissionService : IUserEssaySubmissionService
    {
        private readonly IEssaySubmissionRepository _essaySubmissionRepository;
        private readonly IEssayRepository _essayRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IModuleProgressService _moduleProgressService;
        private readonly IEssayAttachmentService _attachmentService;
        private readonly IGeminiService _geminiService;
        private readonly IAiResponseParser _responseParser;
        private readonly ICourseRepository _courseRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserEssaySubmissionService> _logger;

        public UserEssaySubmissionService(
            IEssaySubmissionRepository essaySubmissionRepository,
            IEssayRepository essayRepository,
            IAssessmentRepository assessmentRepository,
            INotificationRepository notificationRepository,
            IModuleProgressService moduleProgressService,
            IEssayAttachmentService attachmentService,
            IGeminiService geminiService,
            IAiResponseParser responseParser,
            ICourseRepository courseRepository,
            IModuleRepository moduleRepository,
            ILessonRepository lessonRepository,
            IMapper mapper,
            ILogger<UserEssaySubmissionService> logger)
        {
            _essaySubmissionRepository = essaySubmissionRepository;
            _essayRepository = essayRepository;
            _assessmentRepository = assessmentRepository;
            _notificationRepository = notificationRepository;
            _moduleProgressService = moduleProgressService;
            _attachmentService = attachmentService;
            _geminiService = geminiService;
            _responseParser = responseParser;
            _courseRepository = courseRepository;
            _moduleRepository = moduleRepository;
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // Tạo notification khi user nộp essay
        private async Task CreateEssaySubmissionNotificationAsync(int userId, string essayTitle)
        {
            try
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = userId,
                    Title = " Nộp bài essay thành công",
                    Message = $"Bạn đã nộp bài essay '{essayTitle}' thành công. Giáo viên sẽ chấm điểm sớm.",
                    Type = NotificationType.AssessmentGraded,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create essay submission notification failed for UserId: {UserId}, EssayTitle: {EssayTitle}. Error: {Error}", 
                    userId, essayTitle, ex.ToString());
            }
        }

        // User nộp bài essay
        public async Task<ServiceResponse<EssaySubmissionDto>> CreateSubmissionAsync(CreateEssaySubmissionDto dto, int userId)
        {
            var response = new ServiceResponse<EssaySubmissionDto>();

            try
            {
                // Kiểm tra essay tồn tại
                var essay = await _essayRepository.GetEssayByIdAsync(dto.EssayId);
                if (essay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Essay không tồn tại";
                    return response;
                }

                // Kiểm tra hạn nộp assessment
                var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Assessment";
                    return response;
                }

                // Check enrollment: User phải enroll vào course để nộp essay
                var module = await _moduleRepository.GetModuleWithCourseAsync(assessment.ModuleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Module";
                    return response;
                }

                var courseId = module.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, userId);
                if (!isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần đăng ký khóa học để nộp Essay này";
                    _logger.LogWarning("User {UserId} attempted to submit essay {EssayId} without enrollment", 
                        userId, dto.EssayId);
                    return response;
                }

                if (assessment.DueAt != null && DateTime.UtcNow > assessment.DueAt)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Assessment đã quá hạn nộp bài";
                    return response;
                }

                // Không cho nộp lại
                var existed = await _essaySubmissionRepository
                    .GetUserSubmissionForEssayAsync(userId, dto.EssayId);

                if (existed != null)
                {
                    response.Success = false;
                    response.StatusCode = 409;
                    response.Message = "Bạn đã nộp bài essay này rồi";
                    return response;
                }

                // Commit file attachment
                string? attachmentKey = null;
                if (!string.IsNullOrWhiteSpace(dto.AttachmentTempKey))
                {
                    try
                    {
                        attachmentKey = await _attachmentService.CommitAttachmentAsync(dto.AttachmentTempKey);
                    }
                    catch (Exception attachEx)
                    {
                        _logger.LogError(attachEx, "Failed to commit essay attachment");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu file đính kèm";
                        return response;
                    }
                }

                // Tạo submission
                var submission = new EssaySubmission
                {
                    EssayId = dto.EssayId,
                    UserId = userId,
                    TextContent = dto.TextContent,
                    AttachmentKey = attachmentKey,
                    AttachmentType = dto.AttachmentType,
                    SubmittedAt = DateTime.UtcNow,
                    Status = SubmissionStatus.Submitted
                };

                var created = await _essaySubmissionRepository.CreateSubmissionAsync(submission);

                // Hoàn thành module nếu có
                if (assessment?.ModuleId != null)
                    await _moduleProgressService.CompleteModuleAsync(userId, assessment.ModuleId);

                // Tạo notification
                await CreateEssaySubmissionNotificationAsync(userId, essay.Title);

                // Map DTO
                var dtoResult = _mapper.Map<EssaySubmissionDto>(created);
                if (!string.IsNullOrWhiteSpace(created.AttachmentKey))
                {
                    dtoResult.AttachmentUrl = _attachmentService.BuildAttachmentUrl(created.AttachmentKey);
                }

                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Nộp bài Essay thành công";
                response.Data = dtoResult;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateSubmission failed for UserId: {UserId}, EssayId: {EssayId}. Error: {Error}", 
                    userId, dto.EssayId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi nộp bài Essay";
                return response;
            }
        }

        // Lấy submission của chính user theo submissionId
        public async Task<ServiceResponse<EssaySubmissionDto>> GetMySubmissionByIdAsync(int submissionId, int userId)
        {
            var response = new ServiceResponse<EssaySubmissionDto>();

            try
            {
                var submission = await _essaySubmissionRepository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Submission không tồn tại";
                    return response;
                }

                if (submission.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không có quyền truy cập submission này";
                    return response;
                }

                var dto = _mapper.Map<EssaySubmissionDto>(submission);
                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    dto.AttachmentUrl = _attachmentService.BuildAttachmentUrl(submission.AttachmentKey);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy submission thành công";
                response.Data = dto;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMySubmissionById failed for SubmissionId: {SubmissionId}, UserId: {UserId}. Error: {Error}", 
                    submissionId, userId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống";
                return response;
            }
        }

        // Lấy submission của user theo essayId
        public async Task<ServiceResponse<EssaySubmissionDto?>> GetMySubmissionForEssayAsync(int userId, int essayId)
        {
            var response = new ServiceResponse<EssaySubmissionDto?>();

            try
            {
                var submission = await _essaySubmissionRepository
                    .GetUserSubmissionForEssayAsync(userId, essayId);

                if (submission == null)
                {
                    response.Success = true;
                    response.StatusCode = 200;
                    response.Message = "User chưa nộp bài";
                    response.Data = null;
                    return response;
                }

                var dto = _mapper.Map<EssaySubmissionDto>(submission);
                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    dto.AttachmentUrl = _attachmentService.BuildAttachmentUrl(submission.AttachmentKey);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy submission thành công";
                response.Data = dto;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMySubmissionForEssay failed for UserId: {UserId}, EssayId: {EssayId}. Error: {Error}", 
                    userId, essayId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống";
                return response;
            }
        }

        // User cập nhật bài nộp
        public async Task<ServiceResponse<EssaySubmissionDto>> UpdateSubmissionAsync(
            int submissionId, UpdateEssaySubmissionDto dto, int userId)
        {
            var response = new ServiceResponse<EssaySubmissionDto>();

            try
            {
                var submission = await _essaySubmissionRepository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Submission không tồn tại";
                    return response;
                }

                if (!await _essaySubmissionRepository.IsUserOwnerOfSubmissionAsync(userId, submissionId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không có quyền cập nhật";
                    return response;
                }

                // Xóa attachment cũ nếu yêu cầu
                if (dto.RemoveAttachment && !string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    await _attachmentService.DeleteAttachmentAsync(submission.AttachmentKey);
                    submission.AttachmentKey = null;
                    submission.AttachmentType = null;
                }

                // Commit attachment mới
                if (!string.IsNullOrWhiteSpace(dto.AttachmentTempKey))
                {
                    try
                    {
                        var attachmentKey = await _attachmentService.CommitAttachmentAsync(dto.AttachmentTempKey);
                        submission.AttachmentKey = attachmentKey;
                        submission.AttachmentType = dto.AttachmentType;
                    }
                    catch (Exception attachEx)
                    {
                        _logger.LogError(attachEx, "Failed to commit new essay attachment");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu file mới";
                        return response;
                    }
                }

                submission.TextContent = dto.TextContent;

                var updated = await _essaySubmissionRepository.UpdateSubmissionAsync(submission);

                var result = _mapper.Map<EssaySubmissionDto>(updated);
                if (!string.IsNullOrWhiteSpace(updated.AttachmentKey))
                {
                    result.AttachmentUrl = _attachmentService.BuildAttachmentUrl(updated.AttachmentKey);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Cập nhật submission thành công";
                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateSubmission failed for SubmissionId: {SubmissionId}, UserId: {UserId}. Error: {Error}", 
                    submissionId, userId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống";
                return response;
            }
        }

        // User xóa bài nộp
        public async Task<ServiceResponse<bool>> DeleteSubmissionAsync(int submissionId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var submission = await _essaySubmissionRepository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Submission không tồn tại";
                    return response;
                }

                if (!await _essaySubmissionRepository.IsUserOwnerOfSubmissionAsync(userId, submissionId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không có quyền xóa";
                    return response;
                }

                await _essaySubmissionRepository.DeleteSubmissionAsync(submissionId);

                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    await _attachmentService.DeleteAttachmentAsync(submission.AttachmentKey);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa submission thành công";
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteSubmission failed for SubmissionId: {SubmissionId}, UserId: {UserId}. Error: {Error}", 
                    submissionId, userId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống";
                return response;
            }
        }

        public async Task<ServiceResponse<EssayGradingResultDto>> RequestAiGradingAsync(int submissionId, int userId)
        {
            var response = new ServiceResponse<EssayGradingResultDto>();

            try
            {
                _logger.LogInformation("👨‍🎓 Student {UserId} requesting AI grading for submission {SubmissionId}", userId, submissionId);

                // 1. Validate submission ownership
                var submission = await _essaySubmissionRepository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài nộp";
                    return response;
                }

                if (submission.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền chấm bài nộp này";
                    return response;
                }

                // 2. Get essay và assessment (chỉ cần đề bài + điểm tối đa)
                var essay = await _essayRepository.GetEssayByIdAsync(submission.EssayId);
                if (essay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy đề bài";
                    return response;
                }

                var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài kiểm tra";
                    return response;
                }

                // 2.5. Kiểm tra Course Type - CHỈ cho phép System Course
                var course = essay.Assessment?.Module?.Lesson?.Course;
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                if (course.Type != CourseType.System)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ khóa học hệ thống mới được yêu cầu AI chấm điểm tự động. Vui lòng liên hệ giáo viên để được chấm điểm.";
                    _logger.LogWarning("⚠️ User {UserId} attempted to request AI grading for Teacher Course {CourseId}", userId, course.CourseId);
                    return response;
                }

                // 3. Validate submission status
                if (submission.Status == SubmissionStatus.Graded && submission.Score != null)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Bài nộp đã được chấm điểm rồi";
                    return response;
                }

                // 4. Check TextContent
                if (string.IsNullOrWhiteSpace(submission.TextContent))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Bài làm chỉ có file đính kèm. AI không thể chấm tự động. Vui lòng liên hệ admin.";
                    return response;
                }

                // 6. Build prompt using centralized prompt builder
                var maxScore = assessment.TotalPoints;
                var prompt = EssayGradingPrompt.BuildPrompt(
                    essay.Title,
                    essay.Description ?? string.Empty,
                    submission.TextContent,
                    maxScore
                );

                // 7. Call Gemini AI
                var geminiResponse = await _geminiService.GenerateContentAsync(prompt);
                if (!geminiResponse.Success)
                {
                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = $"AI grading failed: {geminiResponse.ErrorMessage}";
                    return response;
                }

                // 8. Parse AI response using centralized parser
                var aiResult = _responseParser.ParseGradingResponse(geminiResponse.Content);

                if (aiResult.Score > maxScore)
                {
                    _logger.LogWarning("⚠️ AI score {Score} exceeds max score {MaxScore}, adjusting...", aiResult.Score, maxScore);
                    aiResult.Score = maxScore;
                }

                // 9. Save result
                submission.Score = aiResult.Score;
                submission.Feedback = aiResult.Feedback;
                submission.GradedAt = DateTime.UtcNow;
                submission.Status = SubmissionStatus.Graded;

                await _essaySubmissionRepository.UpdateSubmissionAsync(submission);

                _logger.LogInformation("✅ AI grading completed for submission {SubmissionId}. Score: {Score}/{MaxScore}", submissionId, aiResult.Score, maxScore);

                // 10. Map result
                var result = new EssayGradingResultDto
                {
                    SubmissionId = submissionId,
                    Score = aiResult.Score,
                    MaxScore = maxScore,
                    Feedback = aiResult.Feedback,
                    Breakdown = aiResult.Breakdown,
                    Strengths = aiResult.Strengths,
                    Improvements = aiResult.Improvements,
                    GradedAt = DateTime.UtcNow,
                    GradedByTeacher = false
                };

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Chấm điểm AI thành công";
                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in RequestAiGradingAsync for SubmissionId: {SubmissionId}, UserId: {UserId}", submissionId, userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi chấm điểm";
                return response;
            }
        }

    }
}
