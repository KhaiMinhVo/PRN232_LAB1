using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<ApiResponse<PagedResponse<SemesterResponse>>> GetAllAsync(ListQueryRequest query);
    Task<ApiResponse<SemesterResponse>>                GetByIdAsync(int id);
    Task<ApiResponse<SemesterResponse>>                CreateAsync(CreateSemesterRequest request);
    Task<ApiResponse<SemesterResponse>>                UpdateAsync(int id, UpdateSemesterRequest request);
    Task<ApiResponse<bool>>                            DeleteAsync(int id);
}
