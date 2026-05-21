using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository    _repo;
    private readonly ISemesterRepository  _semesterRepo;
    private readonly ISubjectRepository   _subjectRepo;

    public CourseService(
        ICourseRepository repo,
        ISemesterRepository semesterRepo,
        ISubjectRepository subjectRepo)
    {
        _repo = repo;
        _semesterRepo = semesterRepo;
        _subjectRepo = subjectRepo;
    }

    public async Task<ApiResponse<PagedResponse<CourseResponse>>> GetAllAsync(ListQueryRequest query)
    {
        var expands = query.Expand?.Split(',').Select(e => e.Trim().ToLower()).ToHashSet()
                     ?? new HashSet<string>();
        bool wantSemester = expands.Contains("semester");
        bool wantSubject  = expands.Contains("subject");

        var (items, total) = await _repo.GetAllAsync(
            query.Search, query.Sort, query.Page, query.Size, wantSemester, wantSubject);

        var responses = items.Select(c => MapToResponse(c, wantSemester, wantSubject));

        var result = new PagedResponse<CourseResponse>
        {
            Items = responses,
            Pagination = new PaginationMeta
            {
                Page = query.Page,
                PageSize = query.Size,
                TotalItems = total
            }
        };

        return ApiResponse<PagedResponse<CourseResponse>>.Ok(result);
    }

    public async Task<ApiResponse<CourseResponse>> GetByIdAsync(int id)
    {
        var course = await _repo.GetByIdAsync(id, includeSemester: true, includeSubject: true);
        if (course is null)
            return ApiResponse<CourseResponse>.Fail($"Course {id} not found.");

        return ApiResponse<CourseResponse>.Ok(MapToResponse(course, true, true));
    }

    public async Task<ApiResponse<CourseResponse>> CreateAsync(CreateCourseRequest request)
    {
        if (!await _semesterRepo.ExistsAsync(request.SemesterId))
            return ApiResponse<CourseResponse>.Fail($"Semester {request.SemesterId} not found.");

        if (!await _subjectRepo.ExistsAsync(request.SubjectId))
            return ApiResponse<CourseResponse>.Fail($"Subject {request.SubjectId} not found.");

        var entity = new Course
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId,
            SubjectId  = request.SubjectId
        };

        var created = await _repo.CreateAsync(entity);
        var full    = await _repo.GetByIdAsync(created.CourseId, true, true);
        return ApiResponse<CourseResponse>.Ok(MapToResponse(full!, true, true), "Course created successfully.");
    }

    public async Task<ApiResponse<CourseResponse>> UpdateAsync(int id, UpdateCourseRequest request)
    {
        var course = await _repo.GetByIdAsync(id);
        if (course is null)
            return ApiResponse<CourseResponse>.Fail($"Course {id} not found.");

        if (!await _semesterRepo.ExistsAsync(request.SemesterId))
            return ApiResponse<CourseResponse>.Fail($"Semester {request.SemesterId} not found.");

        if (!await _subjectRepo.ExistsAsync(request.SubjectId))
            return ApiResponse<CourseResponse>.Fail($"Subject {request.SubjectId} not found.");

        course.CourseName = request.CourseName;
        course.SemesterId = request.SemesterId;
        course.SubjectId  = request.SubjectId;

        var updated = await _repo.UpdateAsync(course);
        var full    = await _repo.GetByIdAsync(updated.CourseId, true, true);
        return ApiResponse<CourseResponse>.Ok(MapToResponse(full!, true, true), "Course updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var course = await _repo.GetByIdAsync(id);
        if (course is null)
            return ApiResponse<bool>.Fail($"Course {id} not found.");

        await _repo.DeleteAsync(course);
        return ApiResponse<bool>.Ok(true, "Course deleted successfully.");
    }

    private static CourseResponse MapToResponse(Course c, bool inclSemester, bool inclSubject) => new()
    {
        CourseId   = c.CourseId,
        CourseName = c.CourseName,
        SemesterId = c.SemesterId,
        SubjectId  = c.SubjectId,
        Semester = inclSemester && c.Semester is not null ? new SemesterResponse
        {
            SemesterId   = c.Semester.SemesterId,
            SemesterName = c.Semester.SemesterName,
            StartDate    = c.Semester.StartDate,
            EndDate      = c.Semester.EndDate
        } : null,
        Subject = inclSubject && c.Subject is not null ? new SubjectResponse
        {
            SubjectId   = c.Subject.SubjectId,
            SubjectCode = c.Subject.SubjectCode,
            SubjectName = c.Subject.SubjectName,
            Credit      = c.Subject.Credit
        } : null
    };
}
