using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<ApiResponse<PagedResponse<SubjectResponse>>> GetAllAsync(ListQueryRequest query);
    Task<ApiResponse<SubjectResponse>>                GetByIdAsync(int id);
    Task<ApiResponse<SubjectResponse>>                CreateAsync(CreateSubjectRequest request);
    Task<ApiResponse<SubjectResponse>>                UpdateAsync(int id, UpdateSubjectRequest request);
    Task<ApiResponse<bool>>                           DeleteAsync(int id);
}
