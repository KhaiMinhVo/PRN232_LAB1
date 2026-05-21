using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task<(IEnumerable<Enrollment> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size,
        bool includeStudent, bool includeCourse);
    Task<Enrollment?> GetByIdAsync(int id, bool includeStudent = false, bool includeCourse = false);
    Task<Enrollment> CreateAsync(Enrollment enrollment);
    Task<Enrollment> UpdateAsync(Enrollment enrollment);
    Task DeleteAsync(Enrollment enrollment);
    Task<bool> ExistsAsync(int id);
    Task<bool> DuplicateExistsAsync(int studentId, int courseId, int? excludeId = null);
}
