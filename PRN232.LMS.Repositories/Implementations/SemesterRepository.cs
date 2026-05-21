using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class SemesterRepository : ISemesterRepository
{
    private readonly LmsDbContext _context;

    public SemesterRepository(LmsDbContext context) => _context = context;

    public async Task<(IEnumerable<Semester> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size)
    {
        var query = _context.Semesters.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.ToLower();
            query = query.Where(s => s.SemesterName.ToLower().Contains(kw));
        }

        query = sort switch
        {
            "semesterName"  => query.OrderBy(s => s.SemesterName),
            "-semesterName" => query.OrderByDescending(s => s.SemesterName),
            "startDate"     => query.OrderBy(s => s.StartDate),
            "-startDate"    => query.OrderByDescending(s => s.StartDate),
            _               => query.OrderBy(s => s.SemesterId)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return (items, total);
    }

    public async Task<Semester?> GetByIdAsync(int id)
        => await _context.Semesters.AsNoTracking().FirstOrDefaultAsync(s => s.SemesterId == id);

    public async Task<Semester> CreateAsync(Semester semester)
    {
        _context.Semesters.Add(semester);
        await _context.SaveChangesAsync();
        return semester;
    }

    public async Task<Semester> UpdateAsync(Semester semester)
    {
        _context.Semesters.Update(semester);
        await _context.SaveChangesAsync();
        return semester;
    }

    public async Task DeleteAsync(Semester semester)
    {
        _context.Semesters.Remove(semester);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Semesters.AnyAsync(s => s.SemesterId == id);
}
