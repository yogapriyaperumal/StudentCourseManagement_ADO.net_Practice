
using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repositories;

public class EnrollmentRepository
{
    // Enroll a student and record the action in one transaction.
    public async Task<int> EnrollAsync(int studentId, int courseId)
    {
        await using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            const string checkSql = """
                SELECT COUNT(*)
                FROM Students s
                INNER JOIN Courses c ON c.CourseId = @CourseId
                WHERE s.StudentId = @StudentId
                  AND s.IsActive = 1
                  AND c.IsActive = 1;
                """;

            await using var checkCommand =
                new SqlCommand(checkSql, connection, transaction);

            checkCommand.Parameters.AddWithValue("@StudentId", studentId);
            checkCommand.Parameters.AddWithValue("@CourseId", courseId);

            int validCount = Convert.ToInt32(
                await checkCommand.ExecuteScalarAsync());

            if (validCount == 0)
                throw new InvalidOperationException(
                    "Student or course does not exist or is inactive.");

            const string insertSql = """
                INSERT INTO Enrollments (StudentId, CourseId, Status)
                VALUES (@StudentId, @CourseId, N'Enrolled');

                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """;

            await using var insertCommand =
                new SqlCommand(insertSql, connection, transaction);

            insertCommand.Parameters.AddWithValue("@StudentId", studentId);
            insertCommand.Parameters.AddWithValue("@CourseId", courseId);

            int enrollmentId = Convert.ToInt32(
                await insertCommand.ExecuteScalarAsync());

            await AddAuditAsync(
                connection, transaction,
                enrollmentId, "INSERT",
                $"Student {studentId} enrolled in course {courseId}.");

            await transaction.CommitAsync();
            return enrollmentId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Get all enrollments, including student and course names.
    public Task<List<Enrollment>> GetAllAsync()
    {
        const string sql = """
            SELECT e.EnrollmentId, e.StudentId, s.Name AS StudentName,
                   e.CourseId, c.CourseName, e.EnrolledAt, e.Status
            FROM Enrollments e
            INNER JOIN Students s ON s.StudentId = e.StudentId
            INNER JOIN Courses c ON c.CourseId = e.CourseId
            ORDER BY e.EnrollmentId DESC;
            """;

        return GetListAsync(sql, null);
    }

    // Get enrollments for one student.
    public Task<List<Enrollment>> GetByStudentAsync(int studentId)
    {
        const string sql = """
            SELECT e.EnrollmentId, e.StudentId, s.Name AS StudentName,
                   e.CourseId, c.CourseName, e.EnrolledAt, e.Status
            FROM Enrollments e
            INNER JOIN Students s ON s.StudentId = e.StudentId
            INNER JOIN Courses c ON c.CourseId = e.CourseId
            WHERE e.StudentId = @Id
            ORDER BY e.EnrollmentId DESC;
            """;

        return GetListAsync(sql, studentId);
    }

    // Get students enrolled in one course.
    public Task<List<Enrollment>> GetByCourseAsync(int courseId)
    {
        const string sql = """
            SELECT e.EnrollmentId, e.StudentId, s.Name AS StudentName,
                   e.CourseId, c.CourseName, e.EnrolledAt, e.Status
            FROM Enrollments e
            INNER JOIN Students s ON s.StudentId = e.StudentId
            INNER JOIN Courses c ON c.CourseId = e.CourseId
            WHERE e.CourseId = @Id
            ORDER BY e.EnrollmentId DESC;
            """;

        return GetListAsync(sql, courseId);
    }

    // Shared method for reading enrollment lists.
    private static async Task<List<Enrollment>> GetListAsync(
        string sql, int? id)
    {
        var enrollments = new List<Enrollment>();

        await using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);

        if (id.HasValue)
            command.Parameters.AddWithValue("@Id", id.Value);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            enrollments.Add(new Enrollment
            {
                EnrollmentId = reader.GetInt32(0),
                StudentId = reader.GetInt32(1),
                StudentName = reader.GetString(2),
                CourseId = reader.GetInt32(3),
                CourseName = reader.GetString(4),
                EnrolledAt = reader.GetDateTime(5),
                Status = reader.GetString(6)
            });
        }

        return enrollments;
    }

    // Complete or cancel an active enrollment.
    public async Task<bool> UpdateStatusAsync(
        int enrollmentId, string status)
    {
        if (status != "Completed" && status != "Cancelled")
            throw new ArgumentException("Invalid enrollment status.");

        await using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            const string sql = """
                UPDATE Enrollments
                SET Status = @Status
                WHERE EnrollmentId = @EnrollmentId
                  AND Status = N'Enrolled';
                """;

            await using var command =
                new SqlCommand(sql, connection, transaction);

            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@EnrollmentId", enrollmentId);

            int rows = await command.ExecuteNonQueryAsync();

            if (rows == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await AddAuditAsync(
                connection, transaction, enrollmentId, "UPDATE",
                $"Enrollment status changed to {status}.");

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Insert an audit record using the same transaction.
    private static async Task AddAuditAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int enrollmentId,
        string action,
        string details)
    {
        const string sql = """
            INSERT INTO AuditLogs
                (EntityName, EntityId, Action, Details)
            VALUES
                (N'Enrollment', @EntityId, @Action, @Details);
            """;

        await using var command =
            new SqlCommand(sql, connection, transaction);

        command.Parameters.AddWithValue("@EntityId", enrollmentId);
        command.Parameters.AddWithValue("@Action", action);
        command.Parameters.AddWithValue("@Details", details);

        await command.ExecuteNonQueryAsync();
    }
}
