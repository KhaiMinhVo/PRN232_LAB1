using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<ApiResponse<PagedResponse<CourseResponse>>> GetAllAsync(ListQueryRequest query);
    Task<ApiResponse<CourseResponse>>                GetByIdAsync(int id);
    Task<ApiResponse<CourseResponse>>                CreateAsync(CreateCourseRequest request);
    Task<ApiResponse<CourseResponse>>                UpdateAsync(int id, UpdateCourseRequest request);
    Task<ApiResponse<bool>>                          DeleteAsync(int id);
}
