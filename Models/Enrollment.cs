
namespace StudentCourseManagement.Models;

public class EnrollmentDetails
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = "";

    public int CourseId { get; set; }

    public string CourseName { get; set; } = "";

    public DateTime EnrolledAt { get; set; }

    public string Status { get; set; } = "Enrolled";
}
