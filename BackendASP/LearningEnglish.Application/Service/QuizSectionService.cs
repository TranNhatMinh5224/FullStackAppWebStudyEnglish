using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service
{
    public class QuizSectionService : IQuizSectionService
    {
        private readonly IQuizSectionRepository _quizSectionRepository;
        private readonly IMapper _mapper;

        public QuizSectionService(IQuizSectionRepository quizSectionRepository, IMapper mapper)
        {
            _quizSectionRepository = quizSectionRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<QuizSectionDto>> CreateQuizSectionAsync(CreateQuizSectionDto createDto)
        {
            var response = new ServiceResponse<QuizSectionDto>();

            try
            {
                // Check if quiz exists
                var quiz = await _quizSectionRepository.GetQuizByIdAsync(createDto.QuizId);
                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = "Quiz không tồn tại.";
                    return response;
                }

                var quizSection = _mapper.Map<QuizSection>(createDto);
                var createdQuizSection = await _quizSectionRepository.CreateQuizSectionAsync(quizSection);

                response.Data = _mapper.Map<QuizSectionDto>(createdQuizSection);
                response.Success = true;
                response.Message = "Tạo phần quiz thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi tạo phần quiz: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<QuizSectionDto>> GetQuizSectionByIdAsync(int quizSectionId)
        {
            var response = new ServiceResponse<QuizSectionDto>();

            try
            {
                var quizSection = await _quizSectionRepository.GetQuizSectionByIdAsync(quizSectionId);
                if (quizSection == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy phần quiz.";
                    return response;
                }

                response.Data = _mapper.Map<QuizSectionDto>(quizSection);
                response.Success = true;
                response.Message = "Lấy thông tin phần quiz thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi lấy thông tin phần quiz: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<List<QuizSectionDto>>> GetQuizSectionsByQuizIdAsync(int quizId)
        {
            var response = new ServiceResponse<List<QuizSectionDto>>();

            try
            {
                var quizSections = await _quizSectionRepository.GetQuizSectionsByQuizIdAsync(quizId);
                response.Data = _mapper.Map<List<QuizSectionDto>>(quizSections);
                response.Success = true;
                response.Message = "Lấy danh sách phần quiz thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi lấy danh sách phần quiz: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<QuizSectionDto>> UpdateQuizSectionAsync(int quizSectionId, UpdateQuizSectionDto updateDto)
        {
            var response = new ServiceResponse<QuizSectionDto>();

            try
            {
                var existingQuizSection = await _quizSectionRepository.GetQuizSectionByIdAsync(quizSectionId);
                if (existingQuizSection == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy phần quiz.";
                    return response;
                }

                // Update properties
                existingQuizSection.Title = updateDto.Title;
                existingQuizSection.Description = updateDto.Description;

                var updatedQuizSection = await _quizSectionRepository.UpdateQuizSectionAsync(existingQuizSection);

                response.Data = _mapper.Map<QuizSectionDto>(updatedQuizSection);
                response.Success = true;
                response.Message = "Cập nhật phần quiz thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi cập nhật phần quiz: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteQuizSectionAsync(int quizSectionId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var quizSection = await _quizSectionRepository.GetQuizSectionByIdAsync(quizSectionId);
                if (quizSection == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy phần quiz.";
                    return response;
                }

                // Check if quiz section has quiz groups
                if (quizSection.QuizGroups?.Any() == true)
                {
                    response.Success = false;
                    response.Message = "Không thể xóa phần quiz đã có nhóm câu hỏi. Vui lòng xóa các nhóm câu hỏi trước.";
                    return response;
                }

                var deleted = await _quizSectionRepository.DeleteQuizSectionAsync(quizSectionId);
                if (!deleted)
                {
                    response.Success = false;
                    response.Message = "Không thể xóa phần quiz.";
                    return response;
                }

                response.Data = true;
                response.Success = true;
                response.Message = "Xóa phần quiz thành công.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Có lỗi xảy ra khi xóa phần quiz: {ex.Message}";
            }

            return response;
        }
    }
}
