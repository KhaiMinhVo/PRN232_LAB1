using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IStudentRepository
{
    Task<(IEnumerable<Student> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size);
    Task<Student?> GetByIdAsync(int id);
    Task<Student> CreateAsync(Student student);
    Task<Student> UpdateAsync(Student student);
    Task DeleteAsync(Student student);
    Task<bool> ExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
