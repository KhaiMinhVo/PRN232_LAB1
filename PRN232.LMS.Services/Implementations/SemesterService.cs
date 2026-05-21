using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Implementations;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _repo;

    public SemesterService(ISemesterRepository repo) => _repo = repo;

    public async Task<ApiResponse<PagedResponse<SemesterResponse>>> GetAllAsync(ListQueryRequest query)
    {
        var (items, total) = await _repo.GetAllAsync(query.Search, query.Sort, query.Page, query.Size);

        var responses = items.Select(s => MapToResponse(s));

        var result = new PagedResponse<SemesterResponse>
        {
            Items = responses,
            Pagination = new PaginationMeta
            {
                Page = query.Page,
                PageSize = query.Size,
                TotalItems = total
            }
        };

        return ApiResponse<PagedResponse<SemesterResponse>>.Ok(result);
    }

    public async Task<ApiResponse<SemesterResponse>> GetByIdAsync(int id)
    {
        var semester = await _repo.GetByIdAsync(id);
        if (semester is null)
            return ApiResponse<SemesterResponse>.Fail($"Semester {id} not found.");

        return ApiResponse<SemesterResponse>.Ok(MapToResponse(semester));
    }

    public async Task<ApiResponse<SemesterResponse>> CreateAsync(CreateSemesterRequest request)
    {
        var entity = new Semester
        {
            SemesterName = request.SemesterName,
            StartDate    = request.StartDate,
            EndDate      = request.EndDate
        };

        var created = await _repo.CreateAsync(entity);
        return ApiResponse<SemesterResponse>.Ok(MapToResponse(created), "Semester created successfully.");
    }

    public async Task<ApiResponse<SemesterResponse>> UpdateAsync(int id, UpdateSemesterRequest request)
    {
        var semester = await _repo.GetByIdAsync(id);
        if (semester is null)
            return ApiResponse<SemesterResponse>.Fail($"Semester {id} not found.");

        semester.SemesterName = request.SemesterName;
        semester.StartDate    = request.StartDate;
        semester.EndDate      = request.EndDate;

        var updated = await _repo.UpdateAsync(semester);
        return ApiResponse<SemesterResponse>.Ok(MapToResponse(updated), "Semester updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var semester = await _repo.GetByIdAsync(id);
        if (semester is null)
            return ApiResponse<bool>.Fail($"Semester {id} not found.");

        await _repo.DeleteAsync(semester);
        return ApiResponse<bool>.Ok(true, "Semester deleted successfully.");
    }

    private static SemesterResponse MapToResponse(Semester s) => new()
    {
        SemesterId   = s.SemesterId,
        SemesterName = s.SemesterName,
        StartDate    = s.StartDate,
        EndDate      = s.EndDate
    };
}
