
using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class AuditLogService
{
    private readonly AuditLogRepository _repository;

    public AuditLogService(AuditLogRepository repository)
    {
        _repository = repository;
    }

    public Task<List<AuditLog>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<List<AuditLog>> GetByEntityAsync(string entityName)
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            throw new ArgumentException(
                "Entity name cannot be empty.");
        }

        return _repository.GetByEntityAsync(entityName.Trim());
    }
}
