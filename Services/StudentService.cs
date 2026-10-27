
using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class StudentService
{
    private readonly StudentRepository _repository;

    public StudentService(StudentRepository repository)
    {
        _repository = repository;
    }

    public Task<int> RegisterAsync(Student student)
    {
        Validate(student);
        return _repository.AddAsync(student);
    }

    public Task<List<Student>> GetAllAsync()
        => _repository.GetAllAsync();

    public async Task<Student?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid student ID.");

        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<Student>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Enter a student name.");

        return await _repository.SearchByNameAsync(name.Trim());
    }

    public async Task<List<Student>> GetByDepartmentAsync(int departmentId)
    {
        if (departmentId <= 0)
            throw new ArgumentException("Invalid department ID.");

        return await _repository.GetByDepartmentAsync(departmentId);
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        if (student.StudentId <= 0)
            throw new ArgumentException("Invalid student ID.");

        Validate(student);
        return await _repository.UpdateAsync(student);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid student ID.");

        return await _repository.DeactivateAsync(id);
    }

    private static void Validate(Student student)
    {
        if (string.IsNullOrWhiteSpace(student.Name))
            throw new ArgumentException("Student name is required.");

        if (student.Name.Trim().Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(student.Email))
            throw new ArgumentException("Email is required.");

        if (student.Email.Trim().Length > 255 ||
            !student.Email.Contains('@'))
            throw new ArgumentException("Enter a valid email address.");

        if (student.Age is < 17 or > 60)
            throw new ArgumentOutOfRangeException(
                nameof(student.Age), "Age must be between 17 and 60.");

        if (student.DepartmentId <= 0)
            throw new ArgumentException("Select a valid department.");

        student.Name = student.Name.Trim();
        student.Email = student.Email.Trim();
    }
}
