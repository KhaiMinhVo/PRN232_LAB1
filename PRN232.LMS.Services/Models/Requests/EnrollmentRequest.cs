namespace PRN232.LMS.Services.Models.Requests;

public class CreateEnrollmentRequest
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = "Active";
}

public class UpdateEnrollmentRequest
{
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;
}
