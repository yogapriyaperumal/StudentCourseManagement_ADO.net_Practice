
using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;
using System.Data;

namespace StudentCourseManagement.Repositories;

public class CourseRepository
{
    // 1. Add a course
    public async Task<int> AddAsync(Course course)
    {
        const string sql = """
            INSERT INTO Courses
                (CourseName, DurationMonths, Fee, DepartmentId)
            VALUES
                (@Name, @Duration, @Fee, @DepartmentId);

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        AddParameters(command, course);

        await connection.OpenAsync();

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    // 2. List all courses
    public async Task<List<Course>> GetAllAsync()
    {
        const string sql = """
            SELECT CourseId, CourseName, DurationMonths,
                   Fee, DepartmentId, IsActive
            FROM Courses
            ORDER BY CourseId;
            """;

        List<Course> courses = new();

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            courses.Add(MapCourse(reader));

        return courses;
    }

    // 3. Search by course ID
    public async Task<Course?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT CourseId, CourseName, DurationMonths,
                   Fee, DepartmentId, IsActive
            FROM Courses
            WHERE CourseId = @Id;
            """;

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await connection.OpenAsync();

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapCourse(reader) : null;
    }

    // 4. Search by partial course name
    public async Task<List<Course>> SearchByNameAsync(string name)
    {
        const string sql = """
            SELECT CourseId, CourseName, DurationMonths,
                   Fee, DepartmentId, IsActive
            FROM Courses
            WHERE CourseName LIKE @Name
            ORDER BY CourseName;
            """;

        List<Course> courses = new();

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100)
            .Value = $"%{name}%";

        await connection.OpenAsync();

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            courses.Add(MapCourse(reader));

        return courses;
    }

    // 5. Filter courses by department
    public async Task<List<Course>> GetByDepartmentAsync(int departmentId)
    {
        const string sql = """
            SELECT CourseId, CourseName, DurationMonths,
                   Fee, DepartmentId, IsActive
            FROM Courses
            WHERE DepartmentId = @DepartmentId
            ORDER BY CourseName;
            """;

        List<Course> courses = new();

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@DepartmentId", SqlDbType.Int)
            .Value = departmentId;

        await connection.OpenAsync();

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            courses.Add(MapCourse(reader));

        return courses;
    }

    // 6. Update course details
    public async Task<bool> UpdateAsync(Course course)
    {
        const string sql = """
            UPDATE Courses
            SET CourseName = @Name,
                DurationMonths = @Duration,
                Fee = @Fee,
                DepartmentId = @DepartmentId
            WHERE CourseId = @CourseId;
            """;

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        AddParameters(command, course);

        command.Parameters.Add("@CourseId", SqlDbType.Int)
            .Value = course.CourseId;

        await connection.OpenAsync();

        return await command.ExecuteNonQueryAsync() > 0;
    }

    // 7. Deactivate course
    public async Task<bool> DeactivateAsync(int id)
    {
        const string sql = """
            UPDATE Courses
            SET IsActive = 0
            WHERE CourseId = @Id
              AND IsActive = 1;
            """;

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await connection.OpenAsync();

        return await command.ExecuteNonQueryAsync() > 0;
    }

    // Shared SQL parameters for INSERT and UPDATE
    private static void AddParameters(
        SqlCommand command, Course course)
    {
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100)
            .Value = course.CourseName;

        command.Parameters.Add("@Duration", SqlDbType.Int)
            .Value = course.DurationMonths;

        SqlParameter fee = command.Parameters.Add(
            "@Fee", SqlDbType.Decimal);

        fee.Precision = 10;
        fee.Scale = 2;
        fee.Value = course.Fee;

        command.Parameters.Add("@DepartmentId", SqlDbType.Int)
            .Value = course.DepartmentId;
    }

    // Map SQL result to Course object
    private static Course MapCourse(SqlDataReader reader)
    {
        return new Course
        {
            CourseId = reader.GetInt32(0),
            CourseName = reader.GetString(1),
            DurationMonths = reader.GetInt32(2),
            Fee = reader.GetDecimal(3),
            DepartmentId = reader.GetInt32(4),
            IsActive = reader.GetBoolean(5)
        };
    }
}
