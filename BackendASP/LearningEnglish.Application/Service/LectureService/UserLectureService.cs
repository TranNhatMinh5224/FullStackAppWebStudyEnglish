using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Lecture;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class UserLectureService : IUserLectureService
    {
        private readonly ILectureRepository _lectureRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserLectureService> _logger;

        // Đặt bucket + folder cho media lecture (video, audio, etc.)
        private const string LectureMediaBucket = "lectures";
        private const string LectureMediaFolder = "real";

        public UserLectureService(
            ILectureRepository lectureRepository,
            IMapper mapper,
            ILogger<UserLectureService> logger)
        {
            _lectureRepository = lectureRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // Lấy thông tin lecture với progress của user
        public async Task<ServiceResponse<LectureDto>> GetLectureByIdAsync(int lectureId, int userId)
        {
            var response = new ServiceResponse<LectureDto>();

            try
            {
                var lecture = await _lectureRepository.GetByIdWithDetailsAsync(lectureId);
                if (lecture == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy lecture";
                    return response;
                }

                var lectureDto = _mapper.Map<LectureDto>(lecture);

                // Generate URL từ key cho MediaUrl
                if (!string.IsNullOrWhiteSpace(lectureDto.MediaUrl))
                {
                    lectureDto.MediaUrl = BuildPublicUrl.BuildURL(
                        LectureMediaBucket,
                        lectureDto.MediaUrl
                    );
                }

                
                response.Data = lectureDto;
                response.Message = "Lấy thông tin lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lecture với ID: {LectureId}", lectureId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi lấy thông tin lecture";
            }

            return response;
        }

        // Lấy danh sách lecture theo module với progress của user
        public async Task<ServiceResponse<List<ListLectureDto>>> GetLecturesByModuleIdAsync(int moduleId, int userId)
        {
            var response = new ServiceResponse<List<ListLectureDto>>();

            try
            {
                var lectures = await _lectureRepository.GetByModuleIdWithDetailsAsync(moduleId);
                var lectureDtos = _mapper.Map<List<ListLectureDto>>(lectures);

                // Generate URLs cho tất cả lectures
                foreach (var dto in lectureDtos)
                {
                    if (!string.IsNullOrWhiteSpace(dto.MediaUrl))
                    {
                        dto.MediaUrl = BuildPublicUrl.BuildURL(LectureMediaBucket, dto.MediaUrl);
                    }

                   
                }

                response.Data = lectureDtos;
                response.Message = $"Lấy danh sách {lectures.Count} lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lecture theo ModuleId: {ModuleId}", moduleId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi lấy danh sách lecture";
            }

            return response;
        }

        // Lấy cấu trúc cây lecture theo module với progress của user
        public async Task<ServiceResponse<List<LectureTreeDto>>> GetLectureTreeByModuleIdAsync(int moduleId, int userId)
        {
            var response = new ServiceResponse<List<LectureTreeDto>>();

            try
            {
                var lectures = await _lectureRepository.GetTreeByModuleIdAsync(moduleId);

                // Tạo cấu trúc cây
                var rootLectures = lectures.Where(l => l.ParentLectureId == null).ToList();
                var treeDtos = new List<LectureTreeDto>();

                foreach (var rootLecture in rootLectures)
                {
                    var treeDto = _mapper.Map<LectureTreeDto>(rootLecture);
                    BuildLectureTree(treeDto, lectures);
                    treeDtos.Add(treeDto);
                }

               

                response.Data = treeDtos;
                response.Message = "Lấy cấu trúc cây lecture thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cấu trúc cây lecture theo ModuleId: {ModuleId}", moduleId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi lấy cấu trúc cây lecture";
            }

            return response;
        }

        // Helper method - Xây dựng cấu trúc cây
        private void BuildLectureTree(LectureTreeDto parent, List<Lecture> allLectures)
        {
            var children = allLectures
                .Where(l => l.ParentLectureId == parent.LectureId)
                .OrderBy(l => l.OrderIndex)
                .ToList();

            foreach (var child in children)
            {
                var childDto = _mapper.Map<LectureTreeDto>(child);
                parent.Children.Add(childDto);
                BuildLectureTree(childDto, allLectures);
            }
        }
    }
}
