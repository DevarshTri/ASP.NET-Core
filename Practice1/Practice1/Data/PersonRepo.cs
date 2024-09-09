using Npgsql;
using Practice1.Models;

namespace Practice1.Data
{
    public class PersonRepo
    {
        private readonly string _connection;

        public PersonRepo(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("conn");
        }
        public async Task<IEnumerable<Person>> GetPeopleAsync()
        {
            var persons = new List<Person>();

            using (var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Person WHERE PersonId = @PersonId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            persons.Add(new Person
                            {
                                PersonId = reader.GetInt32(0),
                                PersonName = reader.GetString(1),
                            });
                        }
                    }
                }
            }
            return persons;
        }
        public async Task<Person> GetId(int id)
        {
            Person person = null;

            using (var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Person WHERE PersonId = @PersonId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("PersonId", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            person = new Person
                            {
                                PersonId = reader.GetInt32(0),
                                PersonName = reader.GetString(1),
                            };
                        }
                    }
                }
            }
            return person;
        }
        public async Task AddPerson(Person person)
        {
            using (var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();

                var query = "INSERT INTO Person(PersonName) VALUES (@PersonName)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("PersonId", person.PersonId);
                    command.Parameters.AddWithValue("PersonName", person.PersonName);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task DeletePerson(int id)
        {
            using (var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();

                var query = "DELETE FROM Person WHERE PersonId = @PersonId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("PersonId", id);

                    await command.ExecuteNonQueryAsync();
                }

            }
        }
        public async Task UpdatePerson(Person person)
        {
            using (var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();
                var query = "UPDATE Person SET PersonName = @PersonName WHERE PersonId = @PersonId";

                using(var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("PersonId", person.PersonId);
                    command.Parameters.AddWithValue("PersonName", person.PersonName);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}