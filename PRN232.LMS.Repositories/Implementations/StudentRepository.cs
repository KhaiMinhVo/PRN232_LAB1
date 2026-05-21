using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class StudentRepository : IStudentRepository
{
    private readonly LmsDbContext _context;

    public StudentRepository(LmsDbContext context) => _context = context;

    public async Task<(IEnumerable<Student> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size)
    {
        var query = _context.Students.AsNoTracking().AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.ToLower();
            query = query.Where(s =>
                s.FullName.ToLower().Contains(kw) ||
                s.Email.ToLower().Contains(kw));
        }

        // Sort — hỗ trợ "fullName,-dateOfBirth"
        if (!string.IsNullOrWhiteSpace(sort))
        {
            var parts = sort.Split(',');
            IOrderedQueryable<Student>? ordered = null;
            foreach (var part in parts)
            {
                var desc = part.StartsWith('-');
                var field = part.TrimStart('-').Trim().ToLower();
                ordered = (field, desc, ordered == null) switch
                {
                    ("fullname",    false, true)  => query.OrderBy(s => s.FullName),
                    ("fullname",    true,  true)  => query.OrderByDescending(s => s.FullName),
                    ("email",       false, true)  => query.OrderBy(s => s.Email),
                    ("email",       true,  true)  => query.OrderByDescending(s => s.Email),
                    ("dateofbirth", false, true)  => query.OrderBy(s => s.DateOfBirth),
                    ("dateofbirth", true,  true)  => query.OrderByDescending(s => s.DateOfBirth),
                    ("fullname",    false, false) => ordered!.ThenBy(s => s.FullName),
                    ("fullname",    true,  false) => ordered!.ThenByDescending(s => s.FullName),
                    ("dateofbirth", false, false) => ordered!.ThenBy(s => s.DateOfBirth),
                    ("dateofbirth", true,  false) => ordered!.ThenByDescending(s => s.DateOfBirth),
                    _ => ordered ?? query.OrderBy(s => s.StudentId)
                };
            }
            if (ordered != null) query = ordered;
        }
        else
        {
            query = query.OrderBy(s => s.StudentId);
        }

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return (items, total);
    }

    public async Task<Student?> GetByIdAsync(int id)
        => await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == id);

    public async Task<Student> CreateAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<Student> UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task DeleteAsync(Student student)
    {
        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Students.AnyAsync(s => s.StudentId == id);

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        => await _context.Students.AnyAsync(s =>
            s.Email == email && (excludeId == null || s.StudentId != excludeId));
}
