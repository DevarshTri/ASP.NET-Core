using Expense_App.Models;
using Npgsql;

namespace Expense_App.Data
{
    public class ExpenseRepo
    {
        private readonly string _connection;
        public ExpenseRepo(IConfiguration configuration) 
        {
            _connection = configuration.GetConnectionString("connection");
        }
        public async Task<IEnumerable<Expense>> GetExpensesAsync()
        {
            var expenses = new List<Expense>();

            using (var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("SELECT * FROM expense WHERE Expense_Id  = @Expense_Id ", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                        {
                            expenses.Add(new Expense
                            {
                                Expense_Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Password = reader.GetString(2),
                                Expense_type = reader.GetString(3),
                                Expense_Description = reader.GetString(4),
                                Expense_Amount = reader.GetString(5),
                            });

                        }
                        }
                    }
                }
            return expenses;
            }
        public async Task AddAsync(Expense expense)
        {
            using (var connection = new NpgsqlConnection(_connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(@"INSERT INTO expense 
            (Expense_Id, Username, Password, Expense_type, Expense_Description, Expense_Amount) 
            VALUES (@Expense_Id, @Username, @Password, @Expense_Type, @Expense_Description, @Expense_Amount)", connection))
                {
                    command.Parameters.AddWithValue("@Expense_Id", expense.Expense_Id);
                    command.Parameters.AddWithValue("@Username", expense.Username); // Use consistent casing
                    command.Parameters.AddWithValue("@Password", expense.Password);
                    command.Parameters.AddWithValue("@Expense_Type", expense.Expense_type);
                    command.Parameters.AddWithValue("@Expense_Description", expense.Expense_Description);
                    command.Parameters.AddWithValue("@Expense_Amount", expense.Expense_Amount);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

    }
}

