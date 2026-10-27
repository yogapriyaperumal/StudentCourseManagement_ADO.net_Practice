
using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repositories;

public class EnrollmentRepository
{
    // Enroll a student and write an audit log in one transaction.
    public async Task<int> EnrollAsync(int studentId, int courseId)
    {
        await using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using var transaction = connection.BeginTransaction();

        try
        {
            // Confirm that both the student and course are active.
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

            checkCommand.Parameters.Add("@StudentId", System.Data.SqlDbType.Int)
                .Value = studentId;
            checkCommand.Parameters.Add("@CourseId", System.Data.SqlDbType.Int)
                .Value = courseId;

            int validCount = Convert.ToInt32(
                await checkCommand.ExecuteScalarAsync());

            if (validCount == 0)
            {
                throw new InvalidOperationException(
                    "Student or course does not exist or is inactive.");
            }

            // Insert the enrollment.
            const string insertSql = """
                INSERT INTO Enrollments (StudentId, CourseId, Status)
                VALUES (@StudentId, @CourseId, N'Enrolled');

                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """;

            await using var insertCommand =
                new SqlCommand(insertSql, connection, transaction);

            insertCommand.Parameters.Add("@StudentId", System.Data.SqlDbType.Int)
                .Value = studentId;
            insertCommand.Parameters.Add("@CourseId", System.Data.SqlDbType.Int)
                .Value = courseId;

            int enrollmentId = Convert.ToInt32(
                await insertCommand.ExecuteScalarAsync());

            // Audit log is part of the same transaction.
            await AddAuditAsync(
                connection,
                transaction,
                enrollmentId,
                "INSERT",
                $"Student {studentId} enrolled in course {courseId}.");

            await transaction.CommitAsync();

            return enrollmentId;
        }
        catch
        {
            await RollbackSafelyAsync(transaction);
            throw;
        }
    }

    // View all enrollments with student and course names.
    public Task<List<EnrollmentDetails>> GetAllAsync()
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

    // View enrollments belonging to one student.
    public Task<List<EnrollmentDetails>> GetByStudentAsync(int studentId)
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

    // View students enrolled in one course.
    public Task<List<EnrollmentDetails>> GetByCourseAsync(int courseId)
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

    // Shared method for retrieving enrollment lists.
    private static async Task<List<EnrollmentDetails>> GetListAsync(
        string sql, int? id)
    {
        var enrollments = new List<EnrollmentDetails>();

        await using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);

        if (id.HasValue)
        {
            command.Parameters.Add("@Id", System.Data.SqlDbType.Int)
                .Value = id.Value;
        }

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            enrollments.Add(new EnrollmentDetails
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

    // Complete or cancel an enrollment that is currently active.
    public async Task<bool> UpdateStatusAsync(
        int enrollmentId, string status)
    {
        if (enrollmentId <= 0)
            throw new ArgumentException("Invalid enrollment ID.");

        if (status != "Completed" && status != "Cancelled")
        {
            throw new ArgumentException(
                "Status must be Completed or Cancelled.");
        }

        await using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using var transaction = connection.BeginTransaction();

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

            command.Parameters.Add("@Status", System.Data.SqlDbType.NVarChar, 20)
                .Value = status;
            command.Parameters.Add("@EnrollmentId", System.Data.SqlDbType.Int)
                .Value = enrollmentId;

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await AddAuditAsync(
                connection,
                transaction,
                enrollmentId,
                "UPDATE",
                $"Enrollment status changed to {status}.");

            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await RollbackSafelyAsync(transaction);
            throw;
        }
    }

    // Insert audit information using the existing transaction.
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

        command.Parameters.Add("@EntityId", System.Data.SqlDbType.Int)
            .Value = enrollmentId;
        command.Parameters.Add("@Action", System.Data.SqlDbType.NVarChar, 50)
            .Value = action;
        command.Parameters.Add("@Details", System.Data.SqlDbType.NVarChar, -1)
            .Value = details;

        await command.ExecuteNonQueryAsync();
    }

    // Avoid hiding the original exception if rollback itself fails.
    private static async Task RollbackSafelyAsync(
        SqlTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync();
        }
        catch (SqlException)
        {
            // Preserve the original exception.
        }
        catch (InvalidOperationException)
        {
            // Transaction may already be completed or disconnected.
        }
    }
}
