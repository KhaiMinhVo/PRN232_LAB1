using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class SubjectRepository : ISubjectRepository
{
    private readonly LmsDbContext _context;

    public SubjectRepository(LmsDbContext context) => _context = context;

    public async Task<(IEnumerable<Subject> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size)
    {
        var query = _context.Subjects.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.ToLower();
            query = query.Where(s =>
                s.SubjectCode.ToLower().Contains(kw) ||
                s.SubjectName.ToLower().Contains(kw));
        }

        query = sort switch
        {
            "subjectName"  => query.OrderBy(s => s.SubjectName),
            "-subjectName" => query.OrderByDescending(s => s.SubjectName),
            "subjectCode"  => query.OrderBy(s => s.SubjectCode),
            "-subjectCode" => query.OrderByDescending(s => s.SubjectCode),
            _              => query.OrderBy(s => s.SubjectId)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return (items, total);
    }

    public async Task<Subject?> GetByIdAsync(int id)
        => await _context.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.SubjectId == id);

    public async Task<Subject> CreateAsync(Subject subject)
    {
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
        return subject;
    }

    public async Task<Subject> UpdateAsync(Subject subject)
    {
        _context.Subjects.Update(subject);
        await _context.SaveChangesAsync();
        return subject;
    }

    public async Task DeleteAsync(Subject subject)
    {
        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Subjects.AnyAsync(s => s.SubjectId == id);
}
