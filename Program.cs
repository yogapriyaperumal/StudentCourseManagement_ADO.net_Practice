
using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;
using StudentCourseManagement.Services;

var service = new CourseService(new CourseRepository());

try
{
    Console.WriteLine("1. Add course");
    Console.WriteLine("2. List courses");
    Console.WriteLine("3. Search by ID");
    Console.WriteLine("4. Search by name");
    Console.WriteLine("5. Filter by department");
    Console.WriteLine("6. Deactivate course");
    Console.Write("Choose: ");

    switch (Console.ReadLine())
    {
        case "1":
            Console.Write("Course name: ");
            string name = Console.ReadLine() ?? "";

            Console.Write("Duration in months: ");
            int duration = int.Parse(Console.ReadLine()!);

            Console.Write("Fee: ");
            decimal fee = decimal.Parse(Console.ReadLine()!);

            Console.Write("Department ID: ");
            int departmentId = int.Parse(Console.ReadLine()!);

            int id = await service.AddAsync(new Course
            {
                CourseName = name,
                DurationMonths = duration,
                Fee = fee,
                DepartmentId = departmentId
            });

            Console.WriteLine($"Course added. ID: {id}");
            break;

        case "2":
            foreach (var course in await service.GetAllAsync())
            {
                Console.WriteLine(
                    $"{course.CourseId} | {course.CourseName} | " +
                    $"{course.DurationMonths} months | " +
                    $"{course.Fee:C} | Department {course.DepartmentId} | " +
                    $"{(course.IsActive ? "Active" : "Inactive")}");
            }
            break;

        case "3":
            Console.Write("Course ID: ");
            var courseById = await service.GetByIdAsync(
                int.Parse(Console.ReadLine()!));

            Console.WriteLine(courseById == null
                ? "Course not found."
                : $"{courseById.CourseId} | {courseById.CourseName}");
            break;

        case "4":
            Console.Write("Course name: ");
            foreach (var course in await service.SearchByNameAsync(
                Console.ReadLine() ?? ""))
            {
                Console.WriteLine($"{course.CourseId} | {course.CourseName}");
            }
            break;

        case "5":
            Console.Write("Department ID: ");
            int deptId = int.Parse(Console.ReadLine()!);

            foreach (var course in await service.GetByDepartmentAsync(deptId))
            {
                Console.WriteLine($"{course.CourseId} | {course.CourseName}");
            }
            break;

        case "6":
            Console.Write("Course ID: ");
            int deactivateId = int.Parse(Console.ReadLine()!);

            Console.WriteLine(await service.DeactivateAsync(deactivateId)
                ? "Course deactivated."
                : "Course not found or already inactive.");
            break;

        default:
            Console.WriteLine("Invalid choice.");
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
