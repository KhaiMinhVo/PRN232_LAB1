namespace PRN232.LMS.Services.Models.Requests;

public class CreateSemesterRequest
{
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class UpdateSemesterRequest
{
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
