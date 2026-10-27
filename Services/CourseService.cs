
using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class CourseService
{
    private readonly CourseRepository _repository;

    public CourseService(CourseRepository repository)
    {
        _repository = repository;
    }

    // Add course
    public Task<int> AddAsync(Course course)
    {
        Validate(course);
        return _repository.AddAsync(course);
    }

    // List all courses
    public Task<List<Course>> GetAllAsync()
        => _repository.GetAllAsync();

    // Search by ID
    public async Task<Course?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid course ID.");

        return await _repository.GetByIdAsync(id);
    }

    // Search by name
    public async Task<List<Course>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Enter a course name.");

        return await _repository.SearchByNameAsync(name.Trim());
    }

    // Filter by department
    public async Task<List<Course>> GetByDepartmentAsync(int departmentId)
    {
        if (departmentId <= 0)
            throw new ArgumentException("Invalid department ID.");

        return await _repository.GetByDepartmentAsync(departmentId);
    }

    // Update course
    public async Task<bool> UpdateAsync(Course course)
    {
        if (course.CourseId <= 0)
            throw new ArgumentException("Invalid course ID.");

        Validate(course);

        return await _repository.UpdateAsync(course);
    }

    // Deactivate course
    public async Task<bool> DeactivateAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid course ID.");

        return await _repository.DeactivateAsync(id);
    }

    // Validate course information
    private static void Validate(Course course)
    {
        if (string.IsNullOrWhiteSpace(course.CourseName))
            throw new ArgumentException("Course name is required.");

        if (course.CourseName.Trim().Length > 100)
            throw new ArgumentException(
                "Course name cannot exceed 100 characters.");

        if (course.DurationMonths <= 0)
            throw new ArgumentException(
                "Duration must be greater than zero.");

        if (course.Fee < 0 || course.Fee > 99999999.99m)
            throw new ArgumentException(
                "Fee must be between 0 and 99,999,999.99.");

        if (course.DepartmentId <= 0)
            throw new ArgumentException(
                "Select a valid department.");

        course.CourseName = course.CourseName.Trim();
    }
}
