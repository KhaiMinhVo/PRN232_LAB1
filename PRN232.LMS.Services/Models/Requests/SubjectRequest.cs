namespace PRN232.LMS.Services.Models.Requests;

public class CreateSubjectRequest
{
    public string SubjectCode { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public int Credit { get; set; }
}

public class UpdateSubjectRequest
{
    public string SubjectCode { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public int Credit { get; set; }
}
