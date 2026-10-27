
using StudentCourseManagement.Repositories;
using StudentCourseManagement.Services;

var repository = new AuditLogRepository();
var service = new AuditLogService(repository);

try
{
    var logs = await service.GetAllAsync();

    foreach (var log in logs)
    {
        Console.WriteLine(
            $"Audit ID: {log.AuditId} | " +
            $"Entity: {log.EntityName} | " +
            $"Record ID: {log.EntityId} | " +
            $"Action: {log.Action} | " +
            $"Date: {log.CreatedAt}");

        Console.WriteLine($"Details: {log.Details}");
        Console.WriteLine("--------------------------------");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
