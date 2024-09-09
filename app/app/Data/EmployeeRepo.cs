using app.Models;
using Npgsql;

namespace app.Data
{
    public class EmployeeRepo
    {
        private readonly string _connection;
        public EmployeeRepo(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("conn");
        }
        public async Task<IEnumerable<Employee>> GetEmployees()
        {
            var employees = new List<Employee>();

            using (var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();

                var query = "SELECT e.EmployeeId , e.FirstName , d.DepartmentName FROM Employee e JOIN Department d " +
                    "ON e.DepartmentId = d.DepartmentId";

                using (var command = new NpgsqlCommand(query,connection))
                {
                    using(var reader = await command.ExecuteReaderAsync())
                    {
                        while(await  reader.ReadAsync())
                        {
                            employees.Add(new Employee
                            {
                                EmployeeId = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                //LastName = reader.GetString(2),
                                //Email = reader.GetString(3),
                                //DepartmentId = reader.GetInt32(4),
                                DepartmentName = reader.GetString(2),
                            });
                        }
                    }
                }
            }
            return employees;
        }
    }
}
