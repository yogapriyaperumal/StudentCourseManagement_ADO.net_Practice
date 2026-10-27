
using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class DepartmentService
{
    private readonly DepartmentRepository _repository;

    public DepartmentService(DepartmentRepository repository)
    {
        _repository = repository;
    }

    // Add department after validation
    public Task<int> AddAsync(Department department)
    {
        Validate(department);
        return _repository.AddAsync(department);
    }

    // List departments
    public Task<List<Department>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    // Search by ID
    public async Task<Department?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive.");

        return await _repository.GetByIdAsync(id);
    }

    // Search by name
    public async Task<List<Department>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Enter a department name.");

        return await _repository.SearchByNameAsync(name.Trim());
    }

    // Update department
    public Task<bool> UpdateAsync(Department department)
    {
        if (department.DepartmentId <= 0)
            throw new ArgumentException("Invalid department ID.");

        Validate(department);

        return _repository.UpdateAsync(department);
    }

    // Deactivate department
    public Task<bool> DeactivateAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid department ID.");

        return _repository.DeactivateAsync(id);
    }

    // Shared validation
    private static void Validate(Department department)
    {
        if (string.IsNullOrWhiteSpace(department.DepartmentName))
            throw new ArgumentException(
                "Department name is required.");

        if (department.DepartmentName.Trim().Length > 100)
            throw new ArgumentException(
                "Department name cannot exceed 100 characters.");

        if (department.Location?.Length > 100)
            throw new ArgumentException(
                "Location cannot exceed 100 characters.");

        department.DepartmentName =
            department.DepartmentName.Trim();

        department.Location =
            string.IsNullOrWhiteSpace(department.Location)
                ? null
                : department.Location.Trim();
    }
}
