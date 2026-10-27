
using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;
using StudentCourseManagement.Services;

namespace StudentCourseManagement;

internal class Program
{
    private static readonly DepartmentService DepartmentService =
        new(new DepartmentRepository());

    private static readonly StudentService StudentService =
        new(new StudentRepository());

    private static readonly CourseService CourseService =
        new(new CourseRepository());

    private static readonly EnrollmentService EnrollmentService =
        new(new EnrollmentRepository());


    private static readonly AuditLogService AuditLogService =
        new(new AuditLogRepository());

    private static async Task Main()
    {
        bool running = true;

        while (running)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("   STUDENT COURSE MANAGEMENT SYSTEM");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Department Management");
            Console.WriteLine("2. Student Management");
            Console.WriteLine("3. Course Management");
            Console.WriteLine("4. Enrollment Management");
            Console.WriteLine("5. View Audit Logs");
            Console.WriteLine("0. Exit");
            Console.WriteLine("========================================");

            string choice = ReadText("Enter your choice: ");

            try
            {
                switch (choice)
                {
                    case "1":
                        await DepartmentMenuAsync();
                        break;
                    case "2":
                        await StudentMenuAsync();
                        break;
                    case "3":
                        await CourseMenuAsync();
                        break;
                    case "4":
                        await EnrollmentMenuAsync();
                        break;
                    case "5":
                        await AuditLogMenuAsync();
                        break;
                    case "0":
                        running = false;
                        Console.WriteLine("Application closed.");
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        Pause();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Pause();
            }
        }
    }

    // =====================================================
    // 1. DEPARTMENT MANAGEMENT
    // =====================================================

    private static async Task DepartmentMenuAsync()
    {
        bool back = false;

        while (!back)
        {
            ShowSubMenu("DEPARTMENT MANAGEMENT",
                "1. Add Department",
                "2. View All Departments",
                "3. Search Department by ID",
                "4. Search Department by Name",
                "5. Update Department",
                "6. Deactivate Department",
                "0. Back");

            switch (ReadText("Enter your choice: "))
            {
                case "1":
                    {
                        var department = new Department
                        {
                            DepartmentName = ReadText("Department name: "),
                            Location = ReadOptionalText("Location (optional): "),
                            IsActive = true
                        };

                        int id = await DepartmentService.AddAsync(department);
                        Console.WriteLine($"Department added. ID: {id}");
                        Pause();
                        break;
                    }

                case "2":
                    {
                        var items = await DepartmentService.GetAllAsync();

                        foreach (var d in items)
                        {
                            Console.WriteLine(
                                $"{d.DepartmentId} | {d.DepartmentName} | " +
                                $"{d.Location ?? "N/A"} | Active: {d.IsActive}");
                        }

                        if (items.Count == 0)
                            Console.WriteLine("No departments found.");

                        Pause();
                        break;
                    }

                case "3":
                    {
                        int id = ReadInt("Department ID: ");
                        var d = await DepartmentService.GetByIdAsync(id);

                        if (d == null)
                            Console.WriteLine("Department not found.");
                        else
                            PrintDepartment(d);

                        Pause();
                        break;
                    }

                case "4":
                    {
                        string name = ReadText("Department name to search: ");
                        var items = await DepartmentService.SearchByNameAsync(name);

                        foreach (var d in items)
                            PrintDepartment(d);

                        if (items.Count == 0)
                            Console.WriteLine("No matching departments found.");

                        Pause();
                        break;
                    }

                case "5":
                    {
                        int id = ReadInt("Department ID to update: ");
                        var d = await DepartmentService.GetByIdAsync(id);

                        if (d == null)
                        {
                            Console.WriteLine("Department not found.");
                            Pause();
                            break;
                        }

                        string name = ReadOptionalText(
                            $"New name [{d.DepartmentName}]: ");

                        string location = ReadOptionalText(
                            $"New location [{d.Location ?? "N/A"}]: ");

                        if (!string.IsNullOrWhiteSpace(name))
                            d.DepartmentName = name;

                        if (!string.IsNullOrWhiteSpace(location))
                            d.Location = location;

                        bool updated = await DepartmentService.UpdateAsync(d);
                        Console.WriteLine(updated
                            ? "Department updated."
                            : "Department was not updated.");

                        Pause();
                        break;
                    }

                case "6":
                    {
                        int id = ReadInt("Department ID to deactivate: ");
                        bool result = await DepartmentService.DeactivateAsync(id);

                        Console.WriteLine(result
                            ? "Department deactivated."
                            : "Department not found or already inactive.");

                        Pause();
                        break;
                    }

                case "0":
                    back = true;
                    break;

                default:
                    Console.WriteLine("Invalid choice.");
                    Pause();
                    break;
            }
        }
    }

    private static void PrintDepartment(Department d)
    {
        Console.WriteLine(
            $"ID: {d.DepartmentId} | Name: {d.DepartmentName} | " +
            $"Location: {d.Location ?? "N/A"} | Active: {d.IsActive}");
    }

    // =====================================================
    // 2. STUDENT MANAGEMENT
    // =====================================================

    private static async Task StudentMenuAsync()
    {
        bool back = false;

        while (!back)
        {
            ShowSubMenu("STUDENT MANAGEMENT",
                "1. Register Student",
                "2. View All Students",
                "3. Search Student by ID",
                "4. Search Student by Name",
                "5. Filter by Department",
                "6. Update Student",
                "7. Deactivate Student",
                "0. Back");

            switch (ReadText("Enter your choice: "))
            {
                case "1":
                    {
                        var student = new Student
                        {
                            Name = ReadText("Student name: "),
                            Email = ReadText("Email: "),
                            Age = ReadInt("Age: "),
                            DepartmentId = ReadInt("Department ID: "),
                            AdmissionDate = DateTime.Today,
                            IsActive = true
                        };

                        int id = await StudentService.RegisterAsync(student);
                        Console.WriteLine($"Student registered. ID: {id}");
                        Pause();
                        break;
                    }

                case "2":
                    {
                        var items = await StudentService.GetAllAsync();

                        foreach (var s in items)
                            PrintStudent(s);

                        if (items.Count == 0)
                            Console.WriteLine("No students found.");

                        Pause();
                        break;
                    }

                case "3":
                    {
                        int id = ReadInt("Student ID: ");
                        var s = await StudentService.GetByIdAsync(id);

                        if (s == null)
                            Console.WriteLine("Student not found.");
                        else
                            PrintStudent(s);

                        Pause();
                        break;
                    }

                case "4":
                    {
                        string name = ReadText("Student name to search: ");
                        var items = await StudentService.SearchByNameAsync(name);

                        foreach (var s in items)
                            PrintStudent(s);

                        if (items.Count == 0)
                            Console.WriteLine("No matching students found.");

                        Pause();
                        break;
                    }

                case "5":
                    {
                        int departmentId = ReadInt("Department ID: ");
                        var items =
                            await StudentService.GetByDepartmentAsync(departmentId);

                        foreach (var s in items)
                            PrintStudent(s);

                        if (items.Count == 0)
                            Console.WriteLine("No students found in this department.");

                        Pause();
                        break;
                    }

                case "6":
                    {
                        int id = ReadInt("Student ID to update: ");
                        var s = await StudentService.GetByIdAsync(id);

                        if (s == null)
                        {
                            Console.WriteLine("Student not found.");
                            Pause();
                            break;
                        }

                        string name = ReadOptionalText($"Name [{s.Name}]: ");
                        string email = ReadOptionalText($"Email [{s.Email}]: ");
                        string ageInput = ReadOptionalText($"Age [{s.Age}]: ");
                        string departmentInput =
                            ReadOptionalText($"Department ID [{s.DepartmentId}]: ");

                        if (!string.IsNullOrWhiteSpace(name))
                            s.Name = name;

                        if (!string.IsNullOrWhiteSpace(email))
                            s.Email = email;

                        if (!string.IsNullOrWhiteSpace(ageInput))
                            s.Age = ParseInt(ageInput, "Age");

                        if (!string.IsNullOrWhiteSpace(departmentInput))
                            s.DepartmentId = ParseInt(
                                departmentInput, "Department ID");

                        bool updated = await StudentService.UpdateAsync(s);

                        Console.WriteLine(updated
                            ? "Student updated."
                            : "Student was not updated.");

                        Pause();
                        break;
                    }

                case "7":
                    {
                        int id = ReadInt("Student ID to deactivate: ");
                        bool result = await StudentService.DeactivateAsync(id);

                        Console.WriteLine(result
                            ? "Student deactivated."
                            : "Student not found or already inactive.");

                        Pause();
                        break;
                    }

                case "0":
                    back = true;
                    break;

                default:
                    Console.WriteLine("Invalid choice.");
                    Pause();
                    break;
            }
        }
    }

    private static void PrintStudent(Student s)
    {
        Console.WriteLine(
            $"ID: {s.StudentId} | Name: {s.Name} | Email: {s.Email} | " +
            $"Age: {s.Age} | Department ID: {s.DepartmentId} | " +
            $"Active: {s.IsActive}");
    }

    // =====================================================
    // 3. COURSE MANAGEMENT
    // =====================================================

    private static async Task CourseMenuAsync()
    {
        bool back = false;

        while (!back)
        {
            ShowSubMenu("COURSE MANAGEMENT",
                "1. Add Course",
                "2. View All Courses",
                "3. Search Course by ID",
                "4. Search Course by Name",
                "5. Filter by Department",
                "6. Update Course",
                "7. Deactivate Course",
                "0. Back");

            switch (ReadText("Enter your choice: "))
            {
                case "1":
                    {
                        var course = new Course
                        {
                            CourseName = ReadText("Course name: "),
                            DurationMonths = ReadInt("Duration in months: "),
                            Fee = ReadDecimal("Course fee: "),
                            DepartmentId = ReadInt("Department ID: "),
                            IsActive = true
                        };

                        int id = await CourseService.AddAsync(course);
                        Console.WriteLine($"Course added. ID: {id}");
                        Pause();
                        break;
                    }

                case "2":
                    {
                        var items = await CourseService.GetAllAsync();

                        foreach (var c in items)
                            PrintCourse(c);

                        if (items.Count == 0)
                            Console.WriteLine("No courses found.");

                        Pause();
                        break;
                    }

                case "3":
                    {
                        int id = ReadInt("Course ID: ");
                        var c = await CourseService.GetByIdAsync(id);

                        if (c == null)
                            Console.WriteLine("Course not found.");
                        else
                            PrintCourse(c);

                        Pause();
                        break;
                    }

                case "4":
                    {
                        string name = ReadText("Course name to search: ");
                        var items = await CourseService.SearchByNameAsync(name);

                        foreach (var c in items)
                            PrintCourse(c);

                        if (items.Count == 0)
                            Console.WriteLine("No matching courses found.");

                        Pause();
                        break;
                    }

                case "5":
                    {
                        int departmentId = ReadInt("Department ID: ");
                        var items =
                            await CourseService.GetByDepartmentAsync(departmentId);

                        foreach (var c in items)
                            PrintCourse(c);

                        if (items.Count == 0)
                            Console.WriteLine("No courses found in this department.");

                        Pause();
                        break;
                    }

                case "6":
                    {
                        int id = ReadInt("Course ID to update: ");
                        var c = await CourseService.GetByIdAsync(id);

                        if (c == null)
                        {
                            Console.WriteLine("Course not found.");
                            Pause();
                            break;
                        }

                        string name = ReadOptionalText($"Name [{c.CourseName}]: ");
                        string duration =
                            ReadOptionalText($"Duration in months [{c.DurationMonths}]: ");
                        string fee = ReadOptionalText($"Fee [{c.Fee}]: ");
                        string department =
                            ReadOptionalText($"Department ID [{c.DepartmentId}]: ");

                        if (!string.IsNullOrWhiteSpace(name))
                            c.CourseName = name;

                        if (!string.IsNullOrWhiteSpace(duration))
                            c.DurationMonths = ParseInt(duration, "Duration");

                        if (!string.IsNullOrWhiteSpace(fee))
                            c.Fee = ParseDecimal(fee, "Fee");

                        if (!string.IsNullOrWhiteSpace(department))
                            c.DepartmentId = ParseInt(department, "Department ID");

                        bool updated = await CourseService.UpdateAsync(c);

                        Console.WriteLine(updated
                            ? "Course updated."
                            : "Course was not updated.");

                        Pause();
                        break;
                    }

                case "7":
                    {
                        int id = ReadInt("Course ID to deactivate: ");
                        bool result = await CourseService.DeactivateAsync(id);

                        Console.WriteLine(result
                            ? "Course deactivated."
                            : "Course not found or already inactive.");

                        Pause();
                        break;
                    }

                case "0":
                    back = true;
                    break;

                default:
                    Console.WriteLine("Invalid choice.");
                    Pause();
                    break;
            }
        }
    }

    private static void PrintCourse(Course c)
    {
        Console.WriteLine(
            $"ID: {c.CourseId} | Name: {c.CourseName} | " +
            $"Duration: {c.DurationMonths} months | Fee: {c.Fee:C} | " +
            $"Department ID: {c.DepartmentId} | Active: {c.IsActive}");
    }

    // =====================================================
    // 4. ENROLLMENT MANAGEMENT
    // =====================================================

    private static async Task EnrollmentMenuAsync()
    {
        bool back = false;

        while (!back)
        {
            ShowSubMenu("ENROLLMENT MANAGEMENT",
                "1. Enroll Student in Course",
                "2. View All Enrollments",
                "3. View Student's Courses",
                "4. View Course's Students",
                "5. Complete Enrollment",
                "6. Cancel Enrollment",
                "0. Back");

            switch (ReadText("Enter your choice: "))
            {
                case "1":
                    {
                        int studentId = ReadInt("Student ID: ");
                        int courseId = ReadInt("Course ID: ");

                        int id = await EnrollmentService.EnrollAsync(
                            studentId, courseId);

                        Console.WriteLine($"Enrollment created. ID: {id}");
                        Pause();
                        break;
                    }

                case "2":
                    {
                        var items = await EnrollmentService.GetAllAsync();
                        PrintEnrollments(items);
                        Pause();
                        break;
                    }

                case "3":
                    {
                        int studentId = ReadInt("Student ID: ");
                        var items =
                            await EnrollmentService.GetByStudentAsync(studentId);

                        PrintEnrollments(items);
                        Pause();
                        break;
                    }

                case "4":
                    {
                        int courseId = ReadInt("Course ID: ");
                        var items =
                            await EnrollmentService.GetByCourseAsync(courseId);

                        PrintEnrollments(items);
                        Pause();
                        break;
                    }

                case "5":
                    {
                        int id = ReadInt("Enrollment ID to complete: ");
                        bool result = await EnrollmentService.CompleteAsync(id);

                        Console.WriteLine(result
                            ? "Enrollment completed."
                            : "Enrollment not found or not active.");

                        Pause();
                        break;
                    }

                case "6":
                    {
                        int id = ReadInt("Enrollment ID to cancel: ");
                        bool result = await EnrollmentService.CancelAsync(id);

                        Console.WriteLine(result
                            ? "Enrollment cancelled."
                            : "Enrollment not found or not active.");

                        Pause();
                        break;
                    }

                case "0":
                    back = true;
                    break;

                default:
                    Console.WriteLine("Invalid choice.");
                    Pause();
                    break;
            }
        }
    }

    private static void PrintEnrollments(
        List<EnrollmentDetails> enrollments)
    {
        if (enrollments.Count == 0)
        {
            Console.WriteLine("No enrollments found.");
            return;
        }

        foreach (var e in enrollments)
        {
            Console.WriteLine(
                $"Enrollment ID: {e.EnrollmentId} | " +
                $"Student: {e.StudentName} (ID {e.StudentId}) | " +
                $"Course: {e.CourseName} (ID {e.CourseId}) | " +
                $"Enrolled: {e.EnrolledAt:g} | Status: {e.Status}");
        }
    }

    
    // =====================================================
    // 5. AUDIT LOGS
    // =====================================================

    private static async Task AuditLogMenuAsync()
    {
        bool back = false;

        while (!back)
        {
            ShowSubMenu("AUDIT LOGS",
                "1. View All Audit Logs",
                "2. Filter by Entity Name",
                "0. Back");

            switch (ReadText("Enter your choice: "))
            {
                case "1":
                    {
                        var logs = await AuditLogService.GetAllAsync();
                        PrintAuditLogs(logs);
                        Pause();
                        break;
                    }

                case "2":
                    {
                        string entity = ReadText(
                            "Entity name (e.g. Enrollment): ");

                        var logs =
                            await AuditLogService.GetByEntityAsync(entity);

                        PrintAuditLogs(logs);
                        Pause();
                        break;
                    }

                case "0":
                    back = true;
                    break;

                default:
                    Console.WriteLine("Invalid choice.");
                    Pause();
                    break;
            }
        }
    }

    private static void PrintAuditLogs(List<AuditLog> logs)
    {
        if (logs.Count == 0)
        {
            Console.WriteLine("No audit logs found.");
            return;
        }

        foreach (var log in logs)
        {
            Console.WriteLine(
                $"Audit ID: {log.AuditId} | Entity: {log.EntityName} | " +
                $"Record ID: {log.EntityId} | Action: {log.Action} | " +
                $"Date: {log.CreatedAt:g}");

            Console.WriteLine($"Details: {log.Details ?? "N/A"}");
            Console.WriteLine("----------------------------------------");
        }
    }

    // =====================================================
    // SHARED CONSOLE HELPERS
    // =====================================================

    private static void ShowSubMenu(
        string title, params string[] options)
    {
        Console.Clear();
        Console.WriteLine($"========== {title} ==========");

        foreach (string option in options)
            Console.WriteLine(option);

        Console.WriteLine();
    }

    private static string ReadText(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }

    private static string ReadOptionalText(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }

    private static int ReadInt(string prompt)
    {
        return ParseInt(ReadText(prompt), prompt);
    }

    private static int ParseInt(string input, string fieldName)
    {
        if (int.TryParse(input, out int value))
            return value;

        throw new ArgumentException(
            $"{fieldName} must be a valid integer.");
    }

    private static decimal ReadDecimal(string prompt)
    {
        return ParseDecimal(ReadText(prompt), prompt);
    }

    private static decimal ParseDecimal(string input, string fieldName)
    {
        if (decimal.TryParse(input, out decimal value))
            return value;

        throw new ArgumentException(
            $"{fieldName} must be a valid number.");
    }

    private static void Pause()
    {
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
    }
}
