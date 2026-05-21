using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repo;
    private readonly IStudentRepository    _studentRepo;
    private readonly ICourseRepository     _courseRepo;

    public EnrollmentService(
        IEnrollmentRepository repo,
        IStudentRepository studentRepo,
        ICourseRepository courseRepo)
    {
        _repo = repo;
        _studentRepo = studentRepo;
        _courseRepo = courseRepo;
    }

    public async Task<ApiResponse<PagedResponse<EnrollmentResponse>>> GetAllAsync(ListQueryRequest query)
    {
        var expands = query.Expand?.Split(',').Select(e => e.Trim().ToLower()).ToHashSet()
                     ?? new HashSet<string>();
        bool wantStudent = expands.Contains("student");
        bool wantCourse  = expands.Contains("course");

        var (items, total) = await _repo.GetAllAsync(
            query.Search, query.Sort, query.Page, query.Size, wantStudent, wantCourse);

        var responses = items.Select(e => MapToResponse(e, wantStudent, wantCourse));

        var result = new PagedResponse<EnrollmentResponse>
        {
            Items = responses,
            Pagination = new PaginationMeta
            {
                Page = query.Page,
                PageSize = query.Size,
                TotalItems = total
            }
        };

        return ApiResponse<PagedResponse<EnrollmentResponse>>.Ok(result);
    }

    public async Task<ApiResponse<EnrollmentResponse>> GetByIdAsync(int id)
    {
        var enrollment = await _repo.GetByIdAsync(id, includeStudent: true, includeCourse: true);
        if (enrollment is null)
            return ApiResponse<EnrollmentResponse>.Fail($"Enrollment {id} not found.");

        return ApiResponse<EnrollmentResponse>.Ok(MapToResponse(enrollment, true, true));
    }

    public async Task<ApiResponse<EnrollmentResponse>> CreateAsync(CreateEnrollmentRequest request)
    {
        if (!await _studentRepo.ExistsAsync(request.StudentId))
            return ApiResponse<EnrollmentResponse>.Fail($"Student {request.StudentId} not found.");

        if (!await _courseRepo.ExistsAsync(request.CourseId))
            return ApiResponse<EnrollmentResponse>.Fail($"Course {request.CourseId} not found.");

        if (await _repo.DuplicateExistsAsync(request.StudentId, request.CourseId))
            return ApiResponse<EnrollmentResponse>.Fail("Student is already enrolled in this course.");

        var entity = new Enrollment
        {
            StudentId  = request.StudentId,
            CourseId   = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status     = request.Status
        };

        var created = await _repo.CreateAsync(entity);
        var full    = await _repo.GetByIdAsync(created.EnrollmentId, true, true);
        return ApiResponse<EnrollmentResponse>.Ok(MapToResponse(full!, true, true), "Enrollment created.");
    }

    public async Task<ApiResponse<EnrollmentResponse>> UpdateAsync(int id, UpdateEnrollmentRequest request)
    {
        var enrollment = await _repo.GetByIdAsync(id);
        if (enrollment is null)
            return ApiResponse<EnrollmentResponse>.Fail($"Enrollment {id} not found.");

        enrollment.EnrollDate = request.EnrollDate;
        enrollment.Status     = request.Status;

        var updated = await _repo.UpdateAsync(enrollment);
        var full    = await _repo.GetByIdAsync(updated.EnrollmentId, true, true);
        return ApiResponse<EnrollmentResponse>.Ok(MapToResponse(full!, true, true), "Enrollment updated.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var enrollment = await _repo.GetByIdAsync(id);
        if (enrollment is null)
            return ApiResponse<bool>.Fail($"Enrollment {id} not found.");

        await _repo.DeleteAsync(enrollment);
        return ApiResponse<bool>.Ok(true, "Enrollment deleted.");
    }

    private static EnrollmentResponse MapToResponse(Enrollment e, bool inclStudent, bool inclCourse) => new()
    {
        EnrollmentId = e.EnrollmentId,
        StudentId    = e.StudentId,
        CourseId     = e.CourseId,
        EnrollDate   = e.EnrollDate,
        Status       = e.Status,
        Student = inclStudent && e.Student is not null ? new StudentResponse
        {
            StudentId   = e.Student.StudentId,
            FullName    = e.Student.FullName,
            Email       = e.Student.Email,
            DateOfBirth = e.Student.DateOfBirth
        } : null,
        Course = inclCourse && e.Course is not null ? new CourseResponse
        {
            CourseId   = e.Course.CourseId,
            CourseName = e.Course.CourseName,
            SemesterId = e.Course.SemesterId,
            SubjectId  = e.Course.SubjectId
        } : null
    };
}
