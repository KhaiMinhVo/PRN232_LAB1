using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ISemesterRepository
{
    Task<(IEnumerable<Semester> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size);
    Task<Semester?> GetByIdAsync(int id);
    Task<Semester> CreateAsync(Semester semester);
    Task<Semester> UpdateAsync(Semester semester);
    Task DeleteAsync(Semester semester);
    Task<bool> ExistsAsync(int id);
}
