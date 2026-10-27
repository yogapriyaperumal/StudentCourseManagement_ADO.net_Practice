
using Microsoft.Data.SqlClient;
using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;
using System.Data;

namespace StudentCourseManagement.Repositories;

public class DepartmentRepository
{
    // 1. Add a new department
    public async Task<int> AddAsync(Department department)
    {
        const string sql = """
            INSERT INTO Departments (DepartmentName, Location)
            VALUES (@Name, @Location);

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100)
            .Value = department.DepartmentName;

        command.Parameters.Add("@Location", SqlDbType.NVarChar, 100)
            .Value = (object?)department.Location ?? DBNull.Value;

        await connection.OpenAsync();

        return Convert.ToInt32(
            await command.ExecuteScalarAsync());
    }

    // 2. Get all departments
    public async Task<List<Department>> GetAllAsync()
    {
        const string sql = """
            SELECT DepartmentId, DepartmentName, Location, IsActive
            FROM Departments
            ORDER BY DepartmentId;
            """;

        List<Department> departments = new();

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        await connection.OpenAsync();

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            departments.Add(new Department
            {
                DepartmentId = reader.GetInt32(0),
                DepartmentName = reader.GetString(1),
                Location = reader.IsDBNull(2)
                    ? null
                    : reader.GetString(2),
                IsActive = reader.GetBoolean(3)
            });
        }

        return departments;
    }

    // 3. Search department by ID
    public async Task<Department?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT DepartmentId, DepartmentName, Location, IsActive
            FROM Departments
            WHERE DepartmentId = @Id;
            """;

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await connection.OpenAsync();

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Department
            {
                DepartmentId = reader.GetInt32(0),
                DepartmentName = reader.GetString(1),
                Location = reader.IsDBNull(2)
                    ? null
                    : reader.GetString(2),
                IsActive = reader.GetBoolean(3)
            };
        }

        return null;
    }

    // 4. Search departments by partial name
    public async Task<List<Department>> SearchByNameAsync(string name)
    {
        const string sql = """
            SELECT DepartmentId, DepartmentName, Location, IsActive
            FROM Departments
            WHERE DepartmentName LIKE @Name
            ORDER BY DepartmentName;
            """;

        List<Department> departments = new();

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100)
            .Value = $"%{name}%";

        await connection.OpenAsync();

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            departments.Add(new Department
            {
                DepartmentId = reader.GetInt32(0),
                DepartmentName = reader.GetString(1),
                Location = reader.IsDBNull(2)
                    ? null
                    : reader.GetString(2),
                IsActive = reader.GetBoolean(3)
            });
        }

        return departments;
    }

    // 5. Update department details
    public async Task<bool> UpdateAsync(Department department)
    {
        const string sql = """
            UPDATE Departments
            SET DepartmentName = @Name,
                Location = @Location
            WHERE DepartmentId = @Id;
            """;

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@Id", SqlDbType.Int)
            .Value = department.DepartmentId;

        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100)
            .Value = department.DepartmentName;

        command.Parameters.Add("@Location", SqlDbType.NVarChar, 100)
            .Value = (object?)department.Location ?? DBNull.Value;

        await connection.OpenAsync();

        return await command.ExecuteNonQueryAsync() > 0;
    }

    // 6. Deactivate department without deleting it
    public async Task<bool> DeactivateAsync(int id)
    {
        const string sql = """
            UPDATE Departments
            SET IsActive = 0
            WHERE DepartmentId = @Id
              AND IsActive = 1;
            """;

        await using SqlConnection connection =
            DatabaseHelper.CreateConnection();

        await using SqlCommand command = new(sql, connection);

        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await connection.OpenAsync();

        return await command.ExecuteNonQueryAsync() > 0;
    }
}
