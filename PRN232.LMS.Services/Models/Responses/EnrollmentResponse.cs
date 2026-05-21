namespace PRN232.LMS.Services.Models.Responses;

public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;

    // expand=student
    public StudentResponse? Student { get; set; }
    // expand=course
    public CourseResponse? Course { get; set; }
}
