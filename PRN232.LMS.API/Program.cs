using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Implementations;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Implementations;
using PRN232.LMS.Services.Interfaces;
using System.Data.Common;

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

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
    var skipMigrate = false;

    var connection = db.Database.GetDbConnection();
    db.Database.OpenConnection();
    try
    {
        var historyExists = TableExists(connection, "__EFMigrationsHistory");
        var historyHasRows = historyExists && TableHasRows(connection, "__EFMigrationsHistory");
        if (!historyHasRows)
        {
            var sentinelTables = new[] { "Semesters", "Students", "Courses", "Subjects", "Enrollments" };
            foreach (var table in sentinelTables)
            {
                if (TableExists(connection, table))
                {
                    skipMigrate = true;
                    break;
                }
            }
        }
    }
    finally
    {
        db.Database.CloseConnection();
    }

    if (!skipMigrate)
    {
        try
        {
            db.Database.Migrate();
        }
        catch (SqlException ex) when (ex.Number == 2714)
        {
            // DB already provisioned without EF history; skip startup migrations.
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN232 LMS API v1");
    c.RoutePrefix = string.Empty; // Swagger tại root "/"
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

static bool TableExists(DbConnection connection, string tableName)
{
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
    var parameter = command.CreateParameter();
    parameter.ParameterName = "@tableName";
    parameter.Value = tableName;
    command.Parameters.Add(parameter);
    return command.ExecuteScalar() != null;
}

static bool TableHasRows(DbConnection connection, string tableName)
{
    using var command = connection.CreateCommand();
    command.CommandText = $"SELECT TOP (1) 1 FROM [{tableName}]";
    try
    {
        return command.ExecuteScalar() != null;
    }
    catch (SqlException ex) when (ex.Number == 208)
    {
        return false;
    }
}

app.Run();
