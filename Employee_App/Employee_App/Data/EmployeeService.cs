using Employee_App.Models;
using Npgsql;

namespace Employee_App.Data
{
    public class EmployeeService
    {
        private readonly string _connection;
        public EmployeeService(IConfiguration configuration) 
        {
            _connection = configuration.GetConnectionString("conn");
        }
        public async Task<IEnumerable<Employee>> GetEmployeesAsync()
        {
            var employees = new List<Employee>();
            using(var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();
                var query = "SELECT e.*,d.* FROM Employee e JOIN Department d " +
                            "on e.DepartmentId = d.DepartmentId";
                using(var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            employees.Add(new Employee
                            {
                                EmployeeId = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                DepartmentId = reader.GetInt32(3),
                                DepartmentName = reader.GetString(4),
                            });
                        }
                    }
                }

            }
            return employees;
        }
    }
}
