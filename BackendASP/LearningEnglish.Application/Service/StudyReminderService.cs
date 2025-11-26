using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Service
{
    public class StudyReminderService : IStudyReminderService
    {
        private readonly IStudyReminderRepository _studyReminderRepository;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        public StudyReminderService(
            IStudyReminderRepository studyReminderRepository,
            IEmailSender emailSender,
            IMapper mapper)
        {
            _studyReminderRepository = studyReminderRepository;
            _emailSender = emailSender;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<StudyReminderDto>> CreateStudyReminderAsync(CreateStudyReminderDto request)
        {
            var response = new ServiceResponse<StudyReminderDto>();

            try
            {
                var reminder = new StudyReminder
                {
                    UserId = request.UserId,
                    Type = request.Type,
                    Title = request.Title,
                    Message = request.Message,
                    ScheduledTime = request.ScheduledTime,
                    DaysOfWeek = request.DaysOfWeek,
                    TimeZone = request.TimeZone,
                    IsPushEnabled = request.IsPushEnabled,
                    IsEmailEnabled = request.IsEmailEnabled,
                    IsActive = request.IsActive
                };

                await _studyReminderRepository.AddAsync(reminder);

                response.Data = _mapper.Map<StudyReminderDto>(reminder);
                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Tạo nhắc học thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi tạo nhắc học: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<List<StudyReminderDto>>> GetUserStudyRemindersAsync(int userId)
        {
            var response = new ServiceResponse<List<StudyReminderDto>>();

            try
            {
                var reminders = await _studyReminderRepository.GetUserStudyRemindersAsync(userId);
                response.Data = _mapper.Map<List<StudyReminderDto>>(reminders);
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách nhắc học thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi lấy danh sách nhắc học: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<StudyReminderDto>> UpdateStudyReminderAsync(int reminderId, CreateStudyReminderDto request, int userId)
        {
            var response = new ServiceResponse<StudyReminderDto>();

            try
            {
                var existingReminder = await _studyReminderRepository.GetByIdAsync(reminderId);
                if (existingReminder == null || existingReminder.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy nhắc học hoặc không có quyền";
                    return response;
                }

                existingReminder.Type = request.Type;
                existingReminder.Title = request.Title;
                existingReminder.Message = request.Message;
                existingReminder.ScheduledTime = request.ScheduledTime;
                existingReminder.DaysOfWeek = request.DaysOfWeek;
                existingReminder.TimeZone = request.TimeZone;
                existingReminder.IsPushEnabled = request.IsPushEnabled;
                existingReminder.IsEmailEnabled = request.IsEmailEnabled;
                existingReminder.IsActive = request.IsActive;
                existingReminder.UpdatedAt = DateTime.UtcNow;

                await _studyReminderRepository.UpdateAsync(existingReminder);

                response.Data = _mapper.Map<StudyReminderDto>(existingReminder);
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Cập nhật nhắc học thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi cập nhật nhắc học: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteStudyReminderAsync(int reminderId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var reminder = await _studyReminderRepository.GetByIdAsync(reminderId);
                if (reminder == null || reminder.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy nhắc học hoặc không có quyền";
                    return response;
                }

                await _studyReminderRepository.DeleteAsync(reminderId);
                response.Data = true;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa nhắc học thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi xóa nhắc học: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> ToggleStudyReminderAsync(int reminderId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var reminder = await _studyReminderRepository.GetByIdAsync(reminderId);
                if (reminder == null || reminder.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy nhắc học hoặc không có quyền";
                    return response;
                }

                reminder.IsActive = !reminder.IsActive;
                reminder.UpdatedAt = DateTime.UtcNow;

                await _studyReminderRepository.UpdateAsync(reminder);

                response.Data = reminder.IsActive;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = reminder.IsActive ? "Nhắc học đã được kích hoạt" : "Nhắc học đã bị vô hiệu hóa";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi thay đổi trạng thái nhắc học: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> SendReminderNowAsync(int reminderId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var reminder = await _studyReminderRepository.GetByIdAsync(reminderId);
                if (reminder == null || reminder.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy nhắc học hoặc không có quyền";
                    return response;
                }

                // Send email if enabled
                if (reminder.IsEmailEnabled)
                {
                    // TODO: Get user email
                    // await _emailSender.SendEmailAsync(userEmail, reminder.Title, reminder.Message);
                }

                // TODO: Send push notification if enabled
                // if (reminder.IsPushEnabled)
                // {
                //     await _pushNotificationService.SendPushNotificationAsync(reminder.UserId, reminder.Title, reminder.Message);
                // }

                reminder.LastSentAt = DateTime.UtcNow;
                reminder.SentCount++;
                await _studyReminderRepository.UpdateAsync(reminder);

                response.Data = true;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Gửi nhắc thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi gửi nhắc: {ex.Message}";
            }

            return response;
        }
    }
}