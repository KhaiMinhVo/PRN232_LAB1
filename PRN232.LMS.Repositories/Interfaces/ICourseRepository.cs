using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ICourseRepository
{
    Task<(IEnumerable<Course> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size,
        bool includeSemester, bool includeSubject);
    Task<Course?> GetByIdAsync(int id, bool includeSemester = false, bool includeSubject = false);
    Task<Course> CreateAsync(Course course);
    Task<Course> UpdateAsync(Course course);
    Task DeleteAsync(Course course);
    Task<bool> ExistsAsync(int id);
}
