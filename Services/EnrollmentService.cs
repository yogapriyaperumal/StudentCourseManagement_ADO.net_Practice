
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class EnrollmentService
{
    private readonly EnrollmentRepository _repository;

    public EnrollmentService(EnrollmentRepository repository)
    {
        _repository = repository;
    }

    public Task<int> EnrollAsync(int studentId, int courseId)
    {
        if (studentId <= 0)
            throw new ArgumentException("Invalid student ID.");

        if (courseId <= 0)
            throw new ArgumentException("Invalid course ID.");

        return _repository.EnrollAsync(studentId, courseId);
    }
}
