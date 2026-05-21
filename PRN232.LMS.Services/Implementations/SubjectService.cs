using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Implementations;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repo;

    public SubjectService(ISubjectRepository repo) => _repo = repo;

    public async Task<ApiResponse<PagedResponse<SubjectResponse>>> GetAllAsync(ListQueryRequest query)
    {
        var (items, total) = await _repo.GetAllAsync(query.Search, query.Sort, query.Page, query.Size);

        var responses = items.Select(s => MapToResponse(s));

        var result = new PagedResponse<SubjectResponse>
        {
            Items = responses,
            Pagination = new PaginationMeta
            {
                Page = query.Page,
                PageSize = query.Size,
                TotalItems = total
            }
        };

        return ApiResponse<PagedResponse<SubjectResponse>>.Ok(result);
    }

    public async Task<ApiResponse<SubjectResponse>> GetByIdAsync(int id)
    {
        var subject = await _repo.GetByIdAsync(id);
        if (subject is null)
            return ApiResponse<SubjectResponse>.Fail($"Subject {id} not found.");

        return ApiResponse<SubjectResponse>.Ok(MapToResponse(subject));
    }

    public async Task<ApiResponse<SubjectResponse>> CreateAsync(CreateSubjectRequest request)
    {
        var entity = new Subject
        {
            SubjectCode = request.SubjectCode,
            SubjectName = request.SubjectName,
            Credit      = request.Credit
        };

        var created = await _repo.CreateAsync(entity);
        return ApiResponse<SubjectResponse>.Ok(MapToResponse(created), "Subject created successfully.");
    }

    public async Task<ApiResponse<SubjectResponse>> UpdateAsync(int id, UpdateSubjectRequest request)
    {
        var subject = await _repo.GetByIdAsync(id);
        if (subject is null)
            return ApiResponse<SubjectResponse>.Fail($"Subject {id} not found.");

        subject.SubjectCode = request.SubjectCode;
        subject.SubjectName = request.SubjectName;
        subject.Credit      = request.Credit;

        var updated = await _repo.UpdateAsync(subject);
        return ApiResponse<SubjectResponse>.Ok(MapToResponse(updated), "Subject updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var subject = await _repo.GetByIdAsync(id);
        if (subject is null)
            return ApiResponse<bool>.Fail($"Subject {id} not found.");

        await _repo.DeleteAsync(subject);
        return ApiResponse<bool>.Ok(true, "Subject deleted successfully.");
    }

    private static SubjectResponse MapToResponse(Subject s) => new()
    {
        SubjectId   = s.SubjectId,
        SubjectCode = s.SubjectCode,
        SubjectName = s.SubjectName,
        Credit      = s.Credit
    };
}
