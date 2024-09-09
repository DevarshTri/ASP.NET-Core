using Npgsql;
using Student_CRUD_Wihout_Entity.Models;

namespace Student_CRUD_Wihout_Entity.Data
{
    public class PersonRepository
    {
        private readonly string _connection;
        public PersonRepository(IConfiguration configuration) 
        {
            _connection = configuration.GetConnectionString("connection");
        }
        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            var persons = new List<Person>();

            using (var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();
                using(var command = new NpgsqlCommand("SELECT * FROM persons WHERE Id = @Id", connection))
                {
                    using(var reader = await command.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            persons.Add(new Person
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                            });
                        }
                    } 
                }

            }
            return persons;


        }
        public async Task<Person> GetbyIdAsync(int id)
        {
            Person person = null;

            using(var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("SELECT * FROM persons WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("Id", id);
                    using(var reader = await command.ExecuteReaderAsync())
                    {
                        if(await reader.ReadAsync())
                        {
                            person = new Person
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                            };
                        }
                    }
                }
            }
            return person;
        }
        public async Task AddAsync(Person person)
        {
            using(var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("INSERT INTO persons VALUES (@Id, @Name, @Email)", connection))
                {
                    command.Parameters.AddWithValue("Id", person.Id);
                    command.Parameters.AddWithValue("Name", person.Name);
                    command.Parameters.AddWithValue("Email",person.Email);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task UpdateAsync(Person person)
        {
            using (var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("UPDATE persons SET Name = @Name, Email = @Email WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("Id", person.Id);
                    command.Parameters.AddWithValue("Name", person.Name);
                    command.Parameters.AddWithValue("Email", person.Email);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task DeleteAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("DELETE FROM persons WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
