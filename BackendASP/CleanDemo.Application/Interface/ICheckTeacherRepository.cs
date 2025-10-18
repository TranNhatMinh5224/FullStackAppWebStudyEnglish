using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface ICheckRepository
    {
        Task GetInformationTeacherpackageAsync(int teacherId);
        
    }
}