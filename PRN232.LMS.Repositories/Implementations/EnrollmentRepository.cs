using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly LmsDbContext _context;

    public EnrollmentRepository(LmsDbContext context) => _context = context;

    public async Task<(IEnumerable<Enrollment> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size,
        bool includeStudent, bool includeCourse)
    {
        var query = _context.Enrollments.AsNoTracking().AsQueryable();

        if (includeStudent) query = query.Include(e => e.Student);
        if (includeCourse)  query = query.Include(e => e.Course).ThenInclude(c => c.Subject);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.ToLower();
            query = query.Where(e =>
                e.Status.ToLower().Contains(kw) ||
                (includeStudent && e.Student.FullName.ToLower().Contains(kw)) ||
                (includeCourse  && e.Course.CourseName.ToLower().Contains(kw)));
        }

        // Sort
        query = sort switch
        {
            "enrollDate"  => query.OrderBy(e => e.EnrollDate),
            "-enrollDate" => query.OrderByDescending(e => e.EnrollDate),
            "status"      => query.OrderBy(e => e.Status),
            "-status"     => query.OrderByDescending(e => e.Status),
            _             => query.OrderByDescending(e => e.EnrollDate)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return (items, total);
    }

    public async Task<Enrollment?> GetByIdAsync(int id, bool includeStudent = false, bool includeCourse = false)
    {
        var query = _context.Enrollments.AsNoTracking().AsQueryable();
        if (includeStudent) query = query.Include(e => e.Student);
        if (includeCourse)
        {
            query = query.Include(e => e.Course)
                         .ThenInclude(c => c.Subject);
            query = query.Include(e => e.Course)
                         .ThenInclude(c => c.Semester);
        }
        return await query.FirstOrDefaultAsync(e => e.EnrollmentId == id);
    }

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task<Enrollment> UpdateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task DeleteAsync(Enrollment enrollment)
    {
        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Enrollments.AnyAsync(e => e.EnrollmentId == id);

    public async Task<bool> DuplicateExistsAsync(int studentId, int courseId, int? excludeId = null)
        => await _context.Enrollments.AnyAsync(e =>
            e.StudentId == studentId && e.CourseId == courseId &&
            (excludeId == null || e.EnrollmentId != excludeId));
}
