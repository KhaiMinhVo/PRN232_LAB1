using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    Task<ApiResponse<PagedResponse<EnrollmentResponse>>> GetAllAsync(ListQueryRequest query, int? courseId = null);
    Task<ApiResponse<EnrollmentResponse>>                GetByIdAsync(int id);
    Task<ApiResponse<EnrollmentResponse>>                CreateAsync(CreateEnrollmentRequest request);
    Task<ApiResponse<EnrollmentResponse>>                UpdateAsync(int id, UpdateEnrollmentRequest request);
    Task<ApiResponse<bool>>                              DeleteAsync(int id);
}
