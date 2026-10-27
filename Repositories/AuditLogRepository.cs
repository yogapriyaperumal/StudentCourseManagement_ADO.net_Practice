
using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repositories;

public class AuditLogRepository
{
    // Get all audit records, newest first.
    public async Task<List<AuditLog>> GetAllAsync()
    {
        var logs = new List<AuditLog>();

        const string sql = """
            SELECT AuditId, EntityName, EntityId, Action, Details, CreatedAt
            FROM AuditLogs
            ORDER BY CreatedAt DESC, AuditId DESC;
            """;

        await using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            logs.Add(MapAuditLog(reader));
        }

        return logs;
    }

    // Get audit records for a particular entity.
    // Example: EntityName = "Enrollment"
    public async Task<List<AuditLog>> GetByEntityAsync(string entityName)
    {
        var logs = new List<AuditLog>();

        const string sql = """
            SELECT AuditId, EntityName, EntityId, Action, Details, CreatedAt
            FROM AuditLogs
            WHERE EntityName = @EntityName
            ORDER BY CreatedAt DESC, AuditId DESC;
            """;

        await using var connection = DatabaseHelper.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@EntityName", entityName);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            logs.Add(MapAuditLog(reader));
        }

        return logs;
    }

    // Convert one database row into an AuditLog object.
    private static AuditLog MapAuditLog(SqlDataReader reader)
    {
        return new AuditLog
        {
            AuditId = reader.GetInt64(0),
            EntityName = reader.GetString(1),
            EntityId = reader.GetInt32(2),
            Action = reader.GetString(3),
            Details = reader.IsDBNull(4) ? null : reader.GetString(4),
            CreatedAt = reader.GetDateTime(5)
        };
    }
}
