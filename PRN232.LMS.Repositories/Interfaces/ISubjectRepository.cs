using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ISubjectRepository
{
    Task<(IEnumerable<Subject> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size);
    Task<Subject?> GetByIdAsync(int id);
    Task<Subject> CreateAsync(Subject subject);
    Task<Subject> UpdateAsync(Subject subject);
    Task DeleteAsync(Subject subject);
    Task<bool> ExistsAsync(int id);
}
