using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class CourseRepository : ICourseRepository
{
    private readonly LmsDbContext _context;

    public CourseRepository(LmsDbContext context) => _context = context;

    public async Task<(IEnumerable<Course> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size,
        bool includeSemester, bool includeSubject)
    {
        var query = _context.Courses.AsNoTracking().AsQueryable();

        if (includeSemester) query = query.Include(c => c.Semester);
        if (includeSubject)  query = query.Include(c => c.Subject);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.ToLower();
            query = query.Where(c => c.CourseName.ToLower().Contains(kw));
        }

        query = sort switch
        {
            "courseName"  => query.OrderBy(c => c.CourseName),
            "-courseName" => query.OrderByDescending(c => c.CourseName),
            _             => query.OrderBy(c => c.CourseId)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return (items, total);
    }

    public async Task<Course?> GetByIdAsync(int id, bool includeSemester = false, bool includeSubject = false)
    {
        var query = _context.Courses.AsNoTracking().AsQueryable();
        if (includeSemester) query = query.Include(c => c.Semester);
        if (includeSubject)  query = query.Include(c => c.Subject);
        return await query.FirstOrDefaultAsync(c => c.CourseId == id);
    }

    public async Task<Course> CreateAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<Course> UpdateAsync(Course course)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task DeleteAsync(Course course)
    {
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Courses.AnyAsync(c => c.CourseId == id);
}
