using Microsoft.AspNetCore.Mvc;

namespace PRN232.LMS.Services.Models.Requests;

public class ListQueryRequest
{
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    [FromQuery(Name = "sort")]
    public string? Sort { get; set; } // "fullName,-dateOfBirth"

    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "size")]
    public int Size { get; set; } = 10;

    [FromQuery(Name = "fields")]
    public string? Fields { get; set; } // "studentId,fullName,email"

    [FromQuery(Name = "expand")]
    public string? Expand { get; set; } // "student,course"
}
