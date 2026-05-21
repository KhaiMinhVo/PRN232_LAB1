namespace PRN232.LMS.Services.Models.Responses;

public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public PaginationMeta Pagination { get; set; } = new();
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
}
