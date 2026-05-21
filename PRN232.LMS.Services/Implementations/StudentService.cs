using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;

    public StudentService(IStudentRepository repo) => _repo = repo;

    public async Task<ApiResponse<PagedResponse<object>>> GetAllAsync(ListQueryRequest query)
    {
        var (items, total) = await _repo.GetAllAsync(query.Search, query.Sort, query.Page, query.Size);

        // Field selection
        var fields = query.Fields?.Split(',').Select(f => f.Trim().ToLower()).ToHashSet();

        IEnumerable<object> projected = items.Select(s => ProjectStudent(s, fields));

        var result = new PagedResponse<object>
        {
            Items = projected,
            Pagination = new PaginationMeta
            {
                Page = query.Page,
                PageSize = query.Size,
                TotalItems = total
            }
        };

        return ApiResponse<PagedResponse<object>>.Ok(result);
    }

    public async Task<ApiResponse<StudentResponse>> GetByIdAsync(int id)
    {
        var student = await _repo.GetByIdAsync(id);
        if (student is null)
            return ApiResponse<StudentResponse>.Fail($"Student {id} not found.");

        return ApiResponse<StudentResponse>.Ok(MapToResponse(student));
    }

    public async Task<ApiResponse<StudentResponse>> CreateAsync(CreateStudentRequest request)
    {
        if (await _repo.EmailExistsAsync(request.Email))
            return ApiResponse<StudentResponse>.Fail("Email already exists.");

        var entity = new Student
        {
            FullName    = request.FullName,
            Email       = request.Email,
            DateOfBirth = request.DateOfBirth
        };

        var created = await _repo.CreateAsync(entity);
        return ApiResponse<StudentResponse>.Ok(MapToResponse(created), "Student created successfully.");
    }

    public async Task<ApiResponse<StudentResponse>> UpdateAsync(int id, UpdateStudentRequest request)
    {
        var student = await _repo.GetByIdAsync(id);
        if (student is null)
            return ApiResponse<StudentResponse>.Fail($"Student {id} not found.");

        if (await _repo.EmailExistsAsync(request.Email, id))
            return ApiResponse<StudentResponse>.Fail("Email already used by another student.");

        student.FullName    = request.FullName;
        student.Email       = request.Email;
        student.DateOfBirth = request.DateOfBirth;

        var updated = await _repo.UpdateAsync(student);
        return ApiResponse<StudentResponse>.Ok(MapToResponse(updated), "Student updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var student = await _repo.GetByIdAsync(id);
        if (student is null)
            return ApiResponse<bool>.Fail($"Student {id} not found.");

        await _repo.DeleteAsync(student);
        return ApiResponse<bool>.Ok(true, "Student deleted successfully.");
    }

    // --- helpers ---
    private static StudentResponse MapToResponse(Student s) => new()
    {
        StudentId   = s.StudentId,
        FullName    = s.FullName,
        Email       = s.Email,
        DateOfBirth = s.DateOfBirth
    };

    private static object ProjectStudent(Student s, HashSet<string>? fields)
    {
        if (fields is null || fields.Count == 0) return MapToResponse(s);

        var dict = new Dictionary<string, object?>();
        if (fields.Contains("studentid"))   dict["studentId"]   = s.StudentId;
        if (fields.Contains("fullname"))    dict["fullName"]    = s.FullName;
        if (fields.Contains("email"))       dict["email"]       = s.Email;
        if (fields.Contains("dateofbirth")) dict["dateOfBirth"] = s.DateOfBirth;
        return dict;
    }
}
