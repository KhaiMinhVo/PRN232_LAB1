# PRN232 LMS REST API — Hướng dẫn xây dựng toàn bộ dự án

## Mục tiêu

Xây dựng ASP.NET Core Web API cho hệ thống quản lý học tập (LMS) với kiến trúc 3 lớp, deploy bằng Docker, tích hợp Swagger.

---

## 0. Chạy nhanh bằng Docker Compose

### Yêu cầu

- Đã cài Docker Desktop

### Lệnh chạy

```bash
docker compose up --build
```

### Truy cập

- Swagger: http://localhost:8080/
- API base: http://localhost:8080/api

### Lưu ý

- SQL Server chạy trong container `lms-sqlserver`
- API tự động chạy EF Core migrations khi ở môi trường Development (docker-compose)

---

## 1. Cấu trúc Solution

```
PRN232.LMS.sln
├── PRN232.LMS.API/              ← Web API project (Controllers, Program.cs, Docker)
├── PRN232.LMS.Services/         ← Class library (Business logic, Service interfaces)
└── PRN232.LMS.Repositories/     ← Class library (Data access, EF Core, Entity models)
```

### Tạo solution và projects

```bash
dotnet new sln -n PRN232.LMS
dotnet new webapi -n PRN232.LMS.API
dotnet new classlib -n PRN232.LMS.Services
dotnet new classlib -n PRN232.LMS.Repositories

dotnet sln add PRN232.LMS.API/PRN232.LMS.API.csproj
dotnet sln add PRN232.LMS.Services/PRN232.LMS.Services.csproj
dotnet sln add PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj

# References
dotnet add PRN232.LMS.API/PRN232.LMS.API.csproj reference PRN232.LMS.Services/PRN232.LMS.Services.csproj
dotnet add PRN232.LMS.Services/PRN232.LMS.Services.csproj reference PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj
```

---

## 2. NuGet Packages

### PRN232.LMS.Repositories

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### PRN232.LMS.API

```bash
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
```

---

## 3. Cấu trúc thư mục chi tiết

```
PRN232.LMS.Repositories/
├── Entities/
│   ├── Semester.cs
│   ├── Course.cs
│   ├── Subject.cs
│   ├── Student.cs
│   └── Enrollment.cs
├── Data/
│   └── LmsDbContext.cs
├── Interfaces/
│   ├── ISemesterRepository.cs
│   ├── ICourseRepository.cs
│   ├── ISubjectRepository.cs
│   ├── IStudentRepository.cs
│   └── IEnrollmentRepository.cs
└── Implementations/
    ├── SemesterRepository.cs
    ├── CourseRepository.cs
    ├── SubjectRepository.cs
    ├── StudentRepository.cs
    └── EnrollmentRepository.cs

PRN232.LMS.Services/
├── Models/
│   ├── Business/
│   │   ├── StudentBM.cs
│   │   ├── EnrollmentBM.cs
│   │   └── CourseBM.cs
│   ├── Requests/
│   │   ├── StudentRequest.cs
│   │   ├── EnrollmentRequest.cs
│   │   ├── ListQueryRequest.cs    ← dùng chung cho tất cả list API
│   │   └── CourseRequest.cs
│   └── Responses/
│       ├── ApiResponse.cs         ← wrapper chung
│       ├── PagedResponse.cs
│       ├── StudentResponse.cs
│       ├── EnrollmentResponse.cs
│       └── CourseResponse.cs
├── Interfaces/
│   ├── IStudentService.cs
│   ├── IEnrollmentService.cs
│   └── ICourseService.cs
└── Implementations/
    ├── StudentService.cs
    ├── EnrollmentService.cs
    └── CourseService.cs

PRN232.LMS.API/
├── Controllers/
│   ├── StudentsController.cs
│   ├── EnrollmentsController.cs
│   ├── CoursesController.cs
│   ├── SubjectsController.cs
│   └── SemestersController.cs
├── Dockerfile
├── docker-compose.yml
└── Program.cs
```

---

## 4. Entity Models (PRN232.LMS.Repositories/Entities/)

### Semester.cs

```csharp
namespace PRN232.LMS.Repositories.Entities;

public class Semester
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
```

### Subject.cs

```csharp
namespace PRN232.LMS.Repositories.Entities;

public class Subject
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public int Credit { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
```

### Course.cs

```csharp
namespace PRN232.LMS.Repositories.Entities;

public class Course
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
    public int SubjectId { get; set; }

    public Semester Semester { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
```

### Student.cs

```csharp
namespace PRN232.LMS.Repositories.Entities;

public class Student
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
```

### Enrollment.cs

```csharp
namespace PRN232.LMS.Repositories.Entities;

public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = "Active";

    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
```

---

## 5. DbContext (PRN232.LMS.Repositories/Data/LmsDbContext.cs)

```csharp
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Data;

public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options) { }

    public DbSet<Semester>   Semesters   => Set<Semester>();
    public DbSet<Subject>    Subjects    => Set<Subject>();
    public DbSet<Course>     Courses     => Set<Course>();
    public DbSet<Student>    Students    => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Semester)
            .WithMany(s => s.Courses)
            .HasForeignKey(c => c.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Subject)
            .WithMany(s => s.Courses)
            .HasForeignKey(c => c.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

## 6. Response & Request Models (PRN232.LMS.Services/Models/)

### Responses/ApiResponse.cs — Wrapper chung cho mọi API

```csharp
namespace PRN232.LMS.Services.Models.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Request processed successfully")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, object? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}
```

### Responses/PagedResponse.cs — Bao gồm pagination metadata

```csharp
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
```

### Requests/ListQueryRequest.cs — Dùng chung cho mọi list API

```csharp
namespace PRN232.LMS.Services.Models.Requests;

public class ListQueryRequest
{
    public string? Search { get; set; }
    public string? Sort { get; set; }          // "fullName,-dateOfBirth"
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Fields { get; set; }        // "studentId,fullName,email"
    public string? Expand { get; set; }        // "student,course"
}
```

### Responses/StudentResponse.cs

```csharp
namespace PRN232.LMS.Services.Models.Responses;

public class StudentResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}
```

### Responses/EnrollmentResponse.cs

```csharp
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
```

### Responses/CourseResponse.cs

```csharp
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
```

### Requests/StudentRequest.cs

```csharp
namespace PRN232.LMS.Services.Models.Requests;

public class CreateStudentRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}

public class UpdateStudentRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}
```

### Requests/EnrollmentRequest.cs

```csharp
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
```

---

## 7. Repository Layer

### Interfaces/IStudentRepository.cs

```csharp
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IStudentRepository
{
    Task<(IEnumerable<Student> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size);
    Task<Student?> GetByIdAsync(int id);
    Task<Student> CreateAsync(Student student);
    Task<Student> UpdateAsync(Student student);
    Task DeleteAsync(Student student);
    Task<bool> ExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
```

### Implementations/StudentRepository.cs

```csharp
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class StudentRepository : IStudentRepository
{
    private readonly LmsDbContext _context;

    public StudentRepository(LmsDbContext context) => _context = context;

    public async Task<(IEnumerable<Student> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size)
    {
        var query = _context.Students.AsNoTracking().AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.ToLower();
            query = query.Where(s =>
                s.FullName.ToLower().Contains(kw) ||
                s.Email.ToLower().Contains(kw));
        }

        // Sort — hỗ trợ "fullName,-dateOfBirth"
        if (!string.IsNullOrWhiteSpace(sort))
        {
            var parts = sort.Split(',');
            IOrderedQueryable<Student>? ordered = null;
            foreach (var part in parts)
            {
                var desc = part.StartsWith('-');
                var field = part.TrimStart('-').Trim().ToLower();
                ordered = (field, desc, ordered == null) switch
                {
                    ("fullname",    false, true)  => query.OrderBy(s => s.FullName),
                    ("fullname",    true,  true)  => query.OrderByDescending(s => s.FullName),
                    ("email",       false, true)  => query.OrderBy(s => s.Email),
                    ("email",       true,  true)  => query.OrderByDescending(s => s.Email),
                    ("dateofbirth", false, true)  => query.OrderBy(s => s.DateOfBirth),
                    ("dateofbirth", true,  true)  => query.OrderByDescending(s => s.DateOfBirth),
                    ("fullname",    false, false) => ordered!.ThenBy(s => s.FullName),
                    ("fullname",    true,  false) => ordered!.ThenByDescending(s => s.FullName),
                    ("dateofbirth", false, false) => ordered!.ThenBy(s => s.DateOfBirth),
                    ("dateofbirth", true,  false) => ordered!.ThenByDescending(s => s.DateOfBirth),
                    _ => ordered ?? query.OrderBy(s => s.StudentId)
                };
            }
            if (ordered != null) query = ordered;
        }
        else
        {
            query = query.OrderBy(s => s.StudentId);
        }

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return (items, total);
    }

    public async Task<Student?> GetByIdAsync(int id)
        => await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == id);

    public async Task<Student> CreateAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<Student> UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task DeleteAsync(Student student)
    {
        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Students.AnyAsync(s => s.StudentId == id);

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        => await _context.Students.AnyAsync(s =>
            s.Email == email && (excludeId == null || s.StudentId != excludeId));
}
```

### Interfaces/IEnrollmentRepository.cs

```csharp
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task<(IEnumerable<Enrollment> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size,
        bool includeStudent, bool includeCourse);
    Task<Enrollment?> GetByIdAsync(int id, bool includeStudent = false, bool includeCourse = false);
    Task<Enrollment> CreateAsync(Enrollment enrollment);
    Task<Enrollment> UpdateAsync(Enrollment enrollment);
    Task DeleteAsync(Enrollment enrollment);
    Task<bool> ExistsAsync(int id);
    Task<bool> DuplicateExistsAsync(int studentId, int courseId, int? excludeId = null);
}
```

### Implementations/EnrollmentRepository.cs

```csharp
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly LmsDbContext _context;

    public EnrollmentRepository(LmsDbContext context) => _context = context;

    public async Task<(IEnumerable<Enrollment> Items, int Total)> GetAllAsync(
        string? search, string? sort, int page, int size,
        bool includeStudent, bool includeCourse)
    {
        var query = _context.Enrollments.AsNoTracking().AsQueryable();

        if (includeStudent) query = query.Include(e => e.Student);
        if (includeCourse)  query = query.Include(e => e.Course).ThenInclude(c => c.Subject);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.ToLower();
            query = query.Where(e =>
                e.Status.ToLower().Contains(kw) ||
                (includeStudent && e.Student.FullName.ToLower().Contains(kw)) ||
                (includeCourse  && e.Course.CourseName.ToLower().Contains(kw)));
        }

        // Sort
        query = sort switch
        {
            "enrollDate"  => query.OrderBy(e => e.EnrollDate),
            "-enrollDate" => query.OrderByDescending(e => e.EnrollDate),
            "status"      => query.OrderBy(e => e.Status),
            "-status"     => query.OrderByDescending(e => e.Status),
            _             => query.OrderByDescending(e => e.EnrollDate)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return (items, total);
    }

    public async Task<Enrollment?> GetByIdAsync(int id, bool includeStudent = false, bool includeCourse = false)
    {
        var query = _context.Enrollments.AsNoTracking().AsQueryable();
        if (includeStudent) query = query.Include(e => e.Student);
        if (includeCourse)  query = query.Include(e => e.Course).ThenInclude(c => c.Subject)
                                                                 .ThenInclude(c => c.Semester);
        return await query.FirstOrDefaultAsync(e => e.EnrollmentId == id);
    }

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task<Enrollment> UpdateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task DeleteAsync(Enrollment enrollment)
    {
        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Enrollments.AnyAsync(e => e.EnrollmentId == id);

    public async Task<bool> DuplicateExistsAsync(int studentId, int courseId, int? excludeId = null)
        => await _context.Enrollments.AnyAsync(e =>
            e.StudentId == studentId && e.CourseId == courseId &&
            (excludeId == null || e.EnrollmentId != excludeId));
}
```

---

## 8. Service Layer

### Interfaces/IStudentService.cs

```csharp
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
```

### Implementations/StudentService.cs

```csharp
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;

    public StudentService(IStudentRepository repo) => _repo = repo;

    public async Task<ApiResponse<PagedResponse<object>>> GetAllAsync(ListQueryRequest query)
    {
        var (items, total) = await _repo.GetAllAsync(query.Search, query.Sort, query.Page, query.Size);

        // Field selection
        var fields = query.Fields?.Split(',').Select(f => f.Trim().ToLower()).ToHashSet();

        IEnumerable<object> projected = items.Select(s => ProjectStudent(s, fields));

        var result = new PagedResponse<object>
        {
            Items = projected,
            Pagination = new PaginationMeta
            {
                Page = query.Page,
                PageSize = query.Size,
                TotalItems = total
            }
        };

        return ApiResponse<PagedResponse<object>>.Ok(result);
    }

    public async Task<ApiResponse<StudentResponse>> GetByIdAsync(int id)
    {
        var student = await _repo.GetByIdAsync(id);
        if (student is null)
            return ApiResponse<StudentResponse>.Fail($"Student {id} not found.");

        return ApiResponse<StudentResponse>.Ok(MapToResponse(student));
    }

    public async Task<ApiResponse<StudentResponse>> CreateAsync(CreateStudentRequest request)
    {
        if (await _repo.EmailExistsAsync(request.Email))
            return ApiResponse<StudentResponse>.Fail("Email already exists.");

        var entity = new Student
        {
            FullName    = request.FullName,
            Email       = request.Email,
            DateOfBirth = request.DateOfBirth
        };

        var created = await _repo.CreateAsync(entity);
        return ApiResponse<StudentResponse>.Ok(MapToResponse(created), "Student created successfully.");
    }

    public async Task<ApiResponse<StudentResponse>> UpdateAsync(int id, UpdateStudentRequest request)
    {
        var student = await _repo.GetByIdAsync(id);
        if (student is null)
            return ApiResponse<StudentResponse>.Fail($"Student {id} not found.");

        if (await _repo.EmailExistsAsync(request.Email, id))
            return ApiResponse<StudentResponse>.Fail("Email already used by another student.");

        student.FullName    = request.FullName;
        student.Email       = request.Email;
        student.DateOfBirth = request.DateOfBirth;

        var updated = await _repo.UpdateAsync(student);
        return ApiResponse<StudentResponse>.Ok(MapToResponse(updated), "Student updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var student = await _repo.GetByIdAsync(id);
        if (student is null)
            return ApiResponse<bool>.Fail($"Student {id} not found.");

        await _repo.DeleteAsync(student);
        return ApiResponse<bool>.Ok(true, "Student deleted successfully.");
    }

    // --- helpers ---
    private static StudentResponse MapToResponse(Student s) => new()
    {
        StudentId   = s.StudentId,
        FullName    = s.FullName,
        Email       = s.Email,
        DateOfBirth = s.DateOfBirth
    };

    private static object ProjectStudent(Student s, HashSet<string>? fields)
    {
        if (fields is null || fields.Count == 0) return MapToResponse(s);

        var dict = new Dictionary<string, object?>();
        if (fields.Contains("studentid"))   dict["studentId"]   = s.StudentId;
        if (fields.Contains("fullname"))    dict["fullName"]    = s.FullName;
        if (fields.Contains("email"))       dict["email"]       = s.Email;
        if (fields.Contains("dateofbirth")) dict["dateOfBirth"] = s.DateOfBirth;
        return dict;
    }
}
```

### Implementations/EnrollmentService.cs — Implement expand=student,course

```csharp
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repo;
    private readonly IStudentRepository    _studentRepo;
    private readonly ICourseRepository     _courseRepo;

    public EnrollmentService(
        IEnrollmentRepository repo,
        IStudentRepository studentRepo,
        ICourseRepository courseRepo)
    {
        _repo = repo;
        _studentRepo = studentRepo;
        _courseRepo = courseRepo;
    }

    public async Task<ApiResponse<PagedResponse<EnrollmentResponse>>> GetAllAsync(ListQueryRequest query)
    {
        var expands = query.Expand?.Split(',').Select(e => e.Trim().ToLower()).ToHashSet()
                     ?? new HashSet<string>();
        bool wantStudent = expands.Contains("student");
        bool wantCourse  = expands.Contains("course");

        var (items, total) = await _repo.GetAllAsync(
            query.Search, query.Sort, query.Page, query.Size, wantStudent, wantCourse);

        var responses = items.Select(e => MapToResponse(e, wantStudent, wantCourse));

        var result = new PagedResponse<EnrollmentResponse>
        {
            Items = responses,
            Pagination = new PaginationMeta
            {
                Page = query.Page,
                PageSize = query.Size,
                TotalItems = total
            }
        };

        return ApiResponse<PagedResponse<EnrollmentResponse>>.Ok(result);
    }

    public async Task<ApiResponse<EnrollmentResponse>> GetByIdAsync(int id)
    {
        var enrollment = await _repo.GetByIdAsync(id, includeStudent: true, includeCourse: true);
        if (enrollment is null)
            return ApiResponse<EnrollmentResponse>.Fail($"Enrollment {id} not found.");

        return ApiResponse<EnrollmentResponse>.Ok(MapToResponse(enrollment, true, true));
    }

    public async Task<ApiResponse<EnrollmentResponse>> CreateAsync(CreateEnrollmentRequest request)
    {
        if (!await _studentRepo.ExistsAsync(request.StudentId))
            return ApiResponse<EnrollmentResponse>.Fail($"Student {request.StudentId} not found.");

        if (!await _courseRepo.ExistsAsync(request.CourseId))
            return ApiResponse<EnrollmentResponse>.Fail($"Course {request.CourseId} not found.");

        if (await _repo.DuplicateExistsAsync(request.StudentId, request.CourseId))
            return ApiResponse<EnrollmentResponse>.Fail("Student is already enrolled in this course.");

        var entity = new Enrollment
        {
            StudentId  = request.StudentId,
            CourseId   = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status     = request.Status
        };

        var created = await _repo.CreateAsync(entity);
        var full    = await _repo.GetByIdAsync(created.EnrollmentId, true, true);
        return ApiResponse<EnrollmentResponse>.Ok(MapToResponse(full!, true, true), "Enrollment created.");
    }

    public async Task<ApiResponse<EnrollmentResponse>> UpdateAsync(int id, UpdateEnrollmentRequest request)
    {
        var enrollment = await _repo.GetByIdAsync(id);
        if (enrollment is null)
            return ApiResponse<EnrollmentResponse>.Fail($"Enrollment {id} not found.");

        enrollment.EnrollDate = request.EnrollDate;
        enrollment.Status     = request.Status;

        var updated = await _repo.UpdateAsync(enrollment);
        var full    = await _repo.GetByIdAsync(updated.EnrollmentId, true, true);
        return ApiResponse<EnrollmentResponse>.Ok(MapToResponse(full!, true, true), "Enrollment updated.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var enrollment = await _repo.GetByIdAsync(id);
        if (enrollment is null)
            return ApiResponse<bool>.Fail($"Enrollment {id} not found.");

        await _repo.DeleteAsync(enrollment);
        return ApiResponse<bool>.Ok(true, "Enrollment deleted.");
    }

    private static EnrollmentResponse MapToResponse(Enrollment e, bool inclStudent, bool inclCourse) => new()
    {
        EnrollmentId = e.EnrollmentId,
        StudentId    = e.StudentId,
        CourseId     = e.CourseId,
        EnrollDate   = e.EnrollDate,
        Status       = e.Status,
        Student = inclStudent && e.Student is not null ? new StudentResponse
        {
            StudentId   = e.Student.StudentId,
            FullName    = e.Student.FullName,
            Email       = e.Student.Email,
            DateOfBirth = e.Student.DateOfBirth
        } : null,
        Course = inclCourse && e.Course is not null ? new CourseResponse
        {
            CourseId   = e.Course.CourseId,
            CourseName = e.Course.CourseName,
            SemesterId = e.Course.SemesterId,
            SubjectId  = e.Course.SubjectId
        } : null
    };
}
```

---

## 9. Controllers (PRN232.LMS.API/Controllers/)

### StudentsController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service) => _service = service;

    /// <summary>Get students with search, sort, paging, field selection</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ListQueryRequest query)
    {
        var result = await _service.GetAllAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get student by ID with full details</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new student</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.StudentId }, result);
    }

    /// <summary>Update an existing student</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : (result.Message.Contains("not found") ? NotFound(result) : BadRequest(result));
    }

    /// <summary>Delete a student</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
```

### EnrollmentsController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service) => _service = service;

    /// <summary>
    /// Get enrollments with search, sort, paging, expand=student,course
    /// </summary>
    /// <remarks>
    /// Example: GET /api/enrollments?search=active&amp;sort=-enrollDate&amp;page=1&amp;size=20&amp;expand=student,course
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ListQueryRequest query)
    {
        var result = await _service.GetAllAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.EnrollmentId }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEnrollmentRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : (result.Message.Contains("not found") ? NotFound(result) : BadRequest(result));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
```

> Tạo tương tự cho `CoursesController`, `SubjectsController`, `SemestersController` — cùng pattern, thay service và request type.

---

## 10. Program.cs (PRN232.LMS.API)

```csharp
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Implementations;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Implementations;
using PRN232.LMS.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IStudentRepository,    StudentRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ICourseRepository,     CourseRepository>();
builder.Services.AddScoped<ISubjectRepository,    SubjectRepository>();
builder.Services.AddScoped<ISemesterRepository,   SemesterRepository>();

// Services
builder.Services.AddScoped<IStudentService,    StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService,     CourseService>();
builder.Services.AddScoped<ISubjectService,    SubjectService>();
builder.Services.AddScoped<ISemesterService,   SemesterService>();

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "PRN232 LMS API",
        Version     = "v1",
        Description = "Learning Management System REST API"
    });
    // Include XML comments for Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// CORS (optional, cho frontend)
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN232 LMS API v1");
    c.RoutePrefix = string.Empty; // Swagger tại root "/"
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=db,1433;Database=LmsDb;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **Lưu ý**: Khi chạy local (không Docker), đổi `Server=db` thành `Server=localhost`.

### PRN232.LMS.API.csproj — Thêm XML comment generation

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

---

## 11. Docker

### Dockerfile (đặt trong PRN232.LMS.API/)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PRN232.LMS.API/PRN232.LMS.API.csproj",          "PRN232.LMS.API/"]
COPY ["PRN232.LMS.Services/PRN232.LMS.Services.csproj", "PRN232.LMS.Services/"]
COPY ["PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj", "PRN232.LMS.Repositories/"]
RUN dotnet restore "PRN232.LMS.API/PRN232.LMS.API.csproj"
COPY . .
WORKDIR "/src/PRN232.LMS.API"
RUN dotnet build "PRN232.LMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PRN232.LMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PRN232.LMS.API.dll"]
```

### docker-compose.yml (đặt tại root solution)

```yaml
version: '3.8'

services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: lms-sqlserver
    environment:
      SA_PASSWORD: "YourStrong@Password123"
      ACCEPT_EULA: "Y"
      MSSQL_PID: "Express"
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Password123 -Q 'SELECT 1'"]
      interval: 10s
      retries: 10
      start_period: 30s

  api:
    build:
      context: .
      dockerfile: PRN232.LMS.API/Dockerfile
    container_name: lms-api
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db,1433;Database=LmsDb;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;
    depends_on:
      db:
        condition: service_healthy

volumes:
  sqldata:
```

### Lệnh chạy Docker

```bash
# Từ thư mục root của solution
docker-compose up --build

# Kiểm tra
docker ps

# API sẽ chạy tại: http://localhost:8080
# Swagger UI tại:  http://localhost:8080/index.html
```

> Sau khi Docker chạy xong, chạy file `LMS_CreateAndSeed.sql` vào database qua SSMS hoặc Azure Data Studio kết nối `localhost:1433`.

---

## 12. Thứ tự build gợi ý cho AI

1. Tạo solution và 3 projects, thêm references và packages
2. Tạo Entity models trong `Repositories/Entities/`
3. Tạo `LmsDbContext`
4. Tạo Repository interfaces và implementations (Student, Enrollment, Course, Subject, Semester)
5. Tạo `ApiResponse<T>` và `PagedResponse<T>`
6. Tạo tất cả Request/Response models
7. Tạo Service interfaces và implementations
8. Tạo Controllers
9. Cấu hình `Program.cs`
10. Thêm `Dockerfile` và `docker-compose.yml`
11. Chạy `docker-compose up --build`
12. Chạy SQL seed data

---

## 13. Checklist tự kiểm tra trước nộp

- [ ] 3 projects đúng naming: `PRN232.LMS.API`, `PRN232.LMS.Services`, `PRN232.LMS.Repositories`
- [ ] Controllers không chứa business logic (chỉ gọi service và return)
- [ ] Repositories không chứa business logic (chỉ query DB)
- [ ] Không return Entity trực tiếp trong response
- [ ] GET list hỗ trợ đủ: search, sort, paging, fields, expand
- [ ] Response bao gồm pagination metadata
- [ ] Tất cả response dùng `ApiResponse<T>` wrapper
- [ ] HTTP status codes đúng: 200 GET/PUT/DELETE, 201 POST, 404 not found, 400 bad request
- [ ] Swagger mở được và test được từng endpoint
- [ ] `docker-compose up` chạy thành công cả API lẫn DB
- [ ] Dữ liệu seed: 5 semesters, 50 students, 10 subjects, 20 courses, ≥500 enrollments
