namespace PRN232.LMS.Services.Models.Responses;

public class CourseResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
    public int SubjectId { get; set; }

    public SemesterResponse? Semester { get; set; }
    public SubjectResponse? Subject { get; set; }
}

public class SemesterResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SubjectResponse
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public int Credit { get; set; }
}
