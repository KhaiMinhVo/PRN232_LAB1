using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<ApiResponse<PagedResponse<object>>> GetAllAsync(ListQueryRequest query);
    Task<ApiResponse<StudentResponse>>       GetByIdAsync(int id);
    Task<ApiResponse<StudentResponse>>       CreateAsync(CreateStudentRequest request);
    Task<ApiResponse<StudentResponse>>       UpdateAsync(int id, UpdateStudentRequest request);
    Task<ApiResponse<bool>>                  DeleteAsync(int id);
}
