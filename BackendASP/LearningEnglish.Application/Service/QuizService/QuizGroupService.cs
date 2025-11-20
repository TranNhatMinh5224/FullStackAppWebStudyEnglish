using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service
{
    public class QuizGroupService : IQuizGroupService
    {
        private readonly IQuizGroupRepository _quizGroupRepository;
        private readonly IMapper _mapper;

        public QuizGroupService(IQuizGroupRepository quizGroupRepository, IMapper mapper)
        {
            _quizGroupRepository = quizGroupRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<QuizGroupDto>> CreateQuizGroupAsync(CreateQuizGroupDto createDto)
        {
            var response = new ServiceResponse<QuizGroupDto>();

            try
            {
                // Check if quiz section exists
                var quizSection = await _quizGroupRepository.GetQuizSectionByIdAsync(createDto.QuizSectionId);
                if (quizSection == null)
                {
                    response.Success = false;
                    response.Message = "Quiz section không tồn tại.";
                    return response;
                }

                var quizGroup = _mapper.Map<QuizGroup>(createDto);
                var createdQuizGroup = await _quizGroupRepository.CreateQuizGroupAsync(quizGroup);

                response.Data = _mapper.Map<QuizGroupDto>(createdQuizGroup);
                response.Success = true;
                response.Message = "Tạo nhóm câu hỏi thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi tạo nhóm câu hỏi: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<QuizGroupDto>> GetQuizGroupByIdAsync(int quizGroupId)
        {
            var response = new ServiceResponse<QuizGroupDto>();

            try
            {
                var quizGroup = await _quizGroupRepository.GetQuizGroupByIdAsync(quizGroupId);
                if (quizGroup == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy nhóm câu hỏi.";
                    return response;
                }

                response.Data = _mapper.Map<QuizGroupDto>(quizGroup);
                response.Success = true;
                response.Message = "Lấy thông tin nhóm câu hỏi thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi lấy thông tin nhóm câu hỏi: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<List<QuizGroupDto>>> GetQuizGroupsByQuizSectionIdAsync(int quizSectionId)
        {
            var response = new ServiceResponse<List<QuizGroupDto>>();

            try
            {
                var quizGroups = await _quizGroupRepository.GetQuizGroupsByQuizSectionIdAsync(quizSectionId);
                response.Data = _mapper.Map<List<QuizGroupDto>>(quizGroups);
                response.Success = true;
                response.Message = "Lấy danh sách nhóm câu hỏi thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi lấy danh sách nhóm câu hỏi: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<QuizGroupDto>> UpdateQuizGroupAsync(int quizGroupId, UpdateQuizGroupDto updateDto)
        {
            var response = new ServiceResponse<QuizGroupDto>();

            try
            {
                var existingQuizGroup = await _quizGroupRepository.GetQuizGroupByIdAsync(quizGroupId);
                if (existingQuizGroup == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy nhóm câu hỏi.";
                    return response;
                }

                // Update properties
                existingQuizGroup.Name = updateDto.Name;
                existingQuizGroup.Description = updateDto.Description;
                existingQuizGroup.SumScore = updateDto.SumScore;

                var updatedQuizGroup = await _quizGroupRepository.UpdateQuizGroupAsync(existingQuizGroup);

                response.Data = _mapper.Map<QuizGroupDto>(updatedQuizGroup);
                response.Success = true;
                response.Message = "Cập nhật nhóm câu hỏi thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi cập nhật nhóm câu hỏi: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteQuizGroupAsync(int quizGroupId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var quizGroup = await _quizGroupRepository.GetQuizGroupByIdAsync(quizGroupId);
                if (quizGroup == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy nhóm câu hỏi.";
                    return response;
                }

                // Check if quiz group has questions
                if (quizGroup.Questions?.Any() == true)
                {
                    response.Success = false;
                    response.Message = "Không thể xóa nhóm câu hỏi đã có câu hỏi. Vui lòng xóa các câu hỏi trước.";
                    return response;
                }

                var deleted = await _quizGroupRepository.DeleteQuizGroupAsync(quizGroupId);
                if (!deleted)
                {
                    response.Success = false;
                    response.Message = "Không thể xóa nhóm câu hỏi.";
                    return response;
                }

                response.Data = true;
                response.Success = true;
                response.Message = "Xóa nhóm câu hỏi thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi xóa nhóm câu hỏi: {ex.Message}";
            }

            return response;
        }
    }
}
