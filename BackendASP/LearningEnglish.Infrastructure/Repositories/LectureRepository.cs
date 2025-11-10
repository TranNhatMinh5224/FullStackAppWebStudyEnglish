using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class LectureRepository : ILectureRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LectureRepository> _logger;

        public LectureRepository(AppDbContext context, ILogger<LectureRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // + Lấy lecture theo ID
        public async Task<Lecture?> GetByIdAsync(int lectureId)
        {
            try
            {
                return await _context.Lectures
                    .FirstOrDefaultAsync(l => l.LectureId == lectureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lecture với ID: {LectureId}", lectureId);
                throw;
            }
        }

        // + Lấy lecture với thông tin chi tiết
        public async Task<Lecture?> GetByIdWithDetailsAsync(int lectureId)
        {
            try
            {
                return await _context.Lectures
                    .Include(l => l.Module)
                        .ThenInclude(m => m!.Lesson)
                            .ThenInclude(lesson => lesson!.Course)
                    .Include(l => l.Parent)
                    .Include(l => l.Children)
                    .Include(l => l.Assessments)
                    .FirstOrDefaultAsync(l => l.LectureId == lectureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lecture chi tiết với ID: {LectureId}", lectureId);
                throw;
            }
        }

        // + Lấy danh sách lecture theo module
        public async Task<List<Lecture>> GetByModuleIdAsync(int moduleId)
        {
            try
            {
                return await _context.Lectures
                    .Where(l => l.ModuleId == moduleId)
                    .OrderBy(l => l.OrderIndex)
                    .ThenBy(l => l.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lecture theo ModuleId: {ModuleId}", moduleId);
                throw;
            }
        }

        // + Lấy danh sách lecture với thông tin chi tiết
        public async Task<List<Lecture>> GetByModuleIdWithDetailsAsync(int moduleId)
        {
            try
            {
                return await _context.Lectures
                    .Include(l => l.Module)
                    .Include(l => l.Parent)
                    .Include(l => l.Children)
                    .Include(l => l.Assessments)
                    .Where(l => l.ModuleId == moduleId)
                    .OrderBy(l => l.OrderIndex)
                    .ThenBy(l => l.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lecture chi tiết theo ModuleId: {ModuleId}", moduleId);
                throw;
            }
        }

        // + Tạo lecture mới
        public async Task<Lecture> CreateAsync(Lecture lecture)
        {
            try
            {
                lecture.CreatedAt = DateTime.UtcNow;
                lecture.UpdatedAt = DateTime.UtcNow;

                _context.Lectures.Add(lecture);
                await _context.SaveChangesAsync();

                return lecture;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lecture mới: {LectureTitle}", lecture.Title);
                throw;
            }
        }

        // + Cập nhật lecture
        public async Task<Lecture> UpdateAsync(Lecture lecture)
        {
            try
            {
                lecture.UpdatedAt = DateTime.UtcNow;
                _context.Lectures.Update(lecture);
                await _context.SaveChangesAsync();

                return lecture;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật lecture với ID: {LectureId}", lecture.LectureId);
                throw;
            }
        }

        // + Xóa lecture
        public async Task<bool> DeleteAsync(int lectureId)
        {
            try
            {
                var lecture = await GetByIdAsync(lectureId);
                if (lecture == null) return false;

                _context.Lectures.Remove(lecture);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa lecture với ID: {LectureId}", lectureId);
                throw;
            }
        }

        // + Kiểm tra lecture có tồn tại
        public async Task<bool> ExistsAsync(int lectureId)
        {
            try
            {
                return await _context.Lectures.AnyAsync(l => l.LectureId == lectureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra tồn tại lecture với ID: {LectureId}", lectureId);
                throw;
            }
        }

        // + Lấy danh sách con của lecture
        public async Task<List<Lecture>> GetChildrenAsync(int parentLectureId)
        {
            try
            {
                return await _context.Lectures
                    .Where(l => l.ParentLectureId == parentLectureId)
                    .OrderBy(l => l.OrderIndex)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách con của lecture với ID: {ParentLectureId}", parentLectureId);
                throw;
            }
        }

        // + Lấy cấu trúc cây lecture theo module
        public async Task<List<Lecture>> GetTreeByModuleIdAsync(int moduleId)
        {
            try
            {
                return await _context.Lectures
                    .Include(l => l.Children.OrderBy(c => c.OrderIndex))
                    .Where(l => l.ModuleId == moduleId)
                    .OrderBy(l => l.OrderIndex)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cấu trúc cây lecture theo ModuleId: {ModuleId}", moduleId);
                throw;
            }
        }

        // + Kiểm tra lecture có con không
        public async Task<bool> HasChildrenAsync(int lectureId)
        {
            try
            {
                return await _context.Lectures.AnyAsync(l => l.ParentLectureId == lectureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra lecture có con với ID: {LectureId}", lectureId);
                throw;
            }
        }

        // + Kiểm tra parent hợp lệ
        public async Task<bool> IsValidParentAsync(int lectureId, int? parentLectureId)
        {
            try
            {
                if (parentLectureId == null) return true;
                if (lectureId == parentLectureId) return false; // Không thể là parent của chính mình

                var lecture = await GetByIdAsync(lectureId);
                var parent = await GetByIdAsync(parentLectureId.Value);
                
                if (lecture == null || parent == null) return false;
                if (lecture.ModuleId != parent.ModuleId) return false; // Phải cùng module

                // TODO: Kiểm tra không tạo vòng lặp (circular reference)
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra parent hợp lệ: LectureId={LectureId}, ParentId={ParentLectureId}", lectureId, parentLectureId);
                throw;
            }
        }

        // + Lấy OrderIndex lớn nhất
        public async Task<int> GetMaxOrderIndexAsync(int moduleId, int? parentLectureId = null)
        {
            try
            {
                var query = _context.Lectures.Where(l => l.ModuleId == moduleId);
                
                if (parentLectureId.HasValue)
                    query = query.Where(l => l.ParentLectureId == parentLectureId.Value);
                else
                    query = query.Where(l => l.ParentLectureId == null);

                var maxOrder = await query.MaxAsync(l => (int?)l.OrderIndex);
                return maxOrder ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy OrderIndex lớn nhất: ModuleId={ModuleId}, ParentId={ParentLectureId}", moduleId, parentLectureId);
                throw;
            }
        }

        // + Lấy lecture với module và course để kiểm tra quyền
        public async Task<Lecture?> GetLectureWithModuleCourseAsync(int lectureId)
        {
            try
            {
                return await _context.Lectures
                    .Include(l => l.Module)
                        .ThenInclude(m => m!.Lesson)
                            .ThenInclude(lesson => lesson!.Course)
                    .FirstOrDefaultAsync(l => l.LectureId == lectureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lecture với module course để kiểm tra quyền: {LectureId}", lectureId);
                throw;
            }
        }
    }
}
