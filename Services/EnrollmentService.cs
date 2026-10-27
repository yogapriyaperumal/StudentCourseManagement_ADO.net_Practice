
using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class EnrollmentService
{
    private readonly EnrollmentRepository _repository;

    public EnrollmentService(EnrollmentRepository repository)
    {
        _repository = repository;
    }

    // Enroll a student in a course.
    public Task<int> EnrollAsync(int studentId, int courseId)
    {
        ValidateId(studentId, "Student");
        ValidateId(courseId, "Course");

        return _repository.EnrollAsync(studentId, courseId);
    }

    // View all enrollments.
    public Task<List<EnrollmentDetails>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    // View all courses enrolled by a student.
    public Task<List<EnrollmentDetails>> GetByStudentAsync(int studentId)
    {
        ValidateId(studentId, "Student");

        return _repository.GetByStudentAsync(studentId);
    }

    // View all students enrolled in a course.
    public Task<List<EnrollmentDetails>> GetByCourseAsync(int courseId)
    {
        ValidateId(courseId, "Course");

        return _repository.GetByCourseAsync(courseId);
    }

    // Complete an active enrollment.
    public Task<bool> CompleteAsync(int enrollmentId)
    {
        ValidateId(enrollmentId, "Enrollment");

        return _repository.UpdateStatusAsync(
            enrollmentId, "Completed");
    }

    // Cancel an active enrollment.
    public Task<bool> CancelAsync(int enrollmentId)
    {
        ValidateId(enrollmentId, "Enrollment");

        return _repository.UpdateStatusAsync(
            enrollmentId, "Cancelled");
    }

    private static void ValidateId(int id, string entity)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                $"{entity} ID must be greater than zero.");
        }
    }
}
