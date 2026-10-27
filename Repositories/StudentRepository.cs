
using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;
using System.Data;

namespace StudentCourseManagement.Repositories;

public class StudentRepository
{
    // 1. Register a student
    public async Task<int> AddAsync(Student student)
    {
        const string sql = """
            INSERT INTO Students (Name, Email, Age, DepartmentId)
            VALUES (@Name, @Email, @Age, @DepartmentId);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        AddStudentParameters(command, student);

        await connection.OpenAsync();
        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    // 2. List all students
    public async Task<List<Student>> GetAllAsync()
    {
        const string sql = """
            SELECT StudentId, Name, Email, Age,
                   DepartmentId, AdmissionDate, IsActive
            FROM Students
            ORDER BY StudentId;
            """;

        List<Student> students = new();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            students.Add(MapStudent(reader));

        return students;
    }

    // 3. Search by student ID
    public async Task<Student?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT StudentId, Name, Email, Age,
                   DepartmentId, AdmissionDate, IsActive
            FROM Students
            WHERE StudentId = @Id;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapStudent(reader) : null;
    }

    // 4. Search by partial name
    public async Task<List<Student>> SearchByNameAsync(string name)
    {
        const string sql = """
            SELECT StudentId, Name, Email, Age,
                   DepartmentId, AdmissionDate, IsActive
            FROM Students
            WHERE Name LIKE @Name
            ORDER BY Name;
            """;

        List<Student> students = new();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100)
            .Value = $"%{name}%";

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            students.Add(MapStudent(reader));

        return students;
    }

    // 5. Filter by department
    public async Task<List<Student>> GetByDepartmentAsync(int departmentId)
    {
        const string sql = """
            SELECT StudentId, Name, Email, Age,
                   DepartmentId, AdmissionDate, IsActive
            FROM Students
            WHERE DepartmentId = @DepartmentId
            ORDER BY Name;
            """;

        List<Student> students = new();

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@DepartmentId", SqlDbType.Int)
            .Value = departmentId;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            students.Add(MapStudent(reader));

        return students;
    }

    // 6. Update student details
    public async Task<bool> UpdateAsync(Student student)
    {
        const string sql = """
            UPDATE Students
            SET Name = @Name,
                Email = @Email,
                Age = @Age,
                DepartmentId = @DepartmentId
            WHERE StudentId = @StudentId;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);

        AddStudentParameters(command, student);
        command.Parameters.Add("@StudentId", SqlDbType.Int)
            .Value = student.StudentId;

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    // 7. Deactivate a student
    public async Task<bool> DeactivateAsync(int id)
    {
        const string sql = """
            UPDATE Students
            SET IsActive = 0
            WHERE StudentId = @Id AND IsActive = 1;
            """;

        await using SqlConnection connection = DatabaseHelper.CreateConnection();
        await using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync() > 0;
    }

    // Reuse parameter definitions for INSERT and UPDATE
    private static void AddStudentParameters(
        SqlCommand command, Student student)
    {
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100)
            .Value = student.Name;

        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255)
            .Value = student.Email;

        command.Parameters.Add("@Age", SqlDbType.Int)
            .Value = student.Age;

        command.Parameters.Add("@DepartmentId", SqlDbType.Int)
            .Value = student.DepartmentId;
    }

    // Convert a database row into a C# Student object
    private static Student MapStudent(SqlDataReader reader)
    {
        return new Student
        {
            StudentId = reader.GetInt32(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            Age = reader.GetInt32(3),
            DepartmentId = reader.GetInt32(4),
            AdmissionDate = reader.GetDateTime(5),
            IsActive = reader.GetBoolean(6)
        };
    }
}
