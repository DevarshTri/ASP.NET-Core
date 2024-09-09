using DataTransfer.Models;
using Npgsql;
using System.Data.SqlClient;

public class DataTransferService : IDataTransferService
{
    private readonly string _source;
    private readonly string _destination;

    public DataTransferService(IConfiguration configuration)
    {
        _source = configuration.GetConnectionString("source");
        _destination = configuration.GetConnectionString("destination");
    }

    public async Task TransferDataAsync()
    {
        using (var sourceConnection = new NpgsqlConnection(_source))
        using (var destinationConnection = new NpgsqlConnection(_destination))
        {
            await sourceConnection.OpenAsync();
            await destinationConnection.OpenAsync();

            // Check if destination table is empty
            var checkIfEmptyQuery = "SELECT COUNT(*) FROM Person";
            var checkIfEmptyCommand = new NpgsqlCommand(checkIfEmptyQuery, destinationConnection);
            var isEmpty = (long)await checkIfEmptyCommand.ExecuteScalarAsync() == 0;

            if (isEmpty)
            {
                // If destination is empty, insert all records from the source
                var sourceQuery = "SELECT S.LOCATION, S.EZ_STORE_ID, I.MAINUPC, SI.QUANTITY " +
                    "FROM MST_STORE S INNER JOIN STK_FACILITY F ON S.STOREID = F.STOREID INNER JOIN STK_ITEMSTOCK SI ON F.FACILITYID = SI.FACILITYID " +
                    "INNER JOIN ITM_ITEMINFO I ON SI.SKU = I.SKU WHERE S.EZ_STORE_ID IS NOT NULL AND S.EZ_STORE_ID > 0 AND F.ISPREDEFINED = 1 " +
                    "ORDER BY S.EZ_STORE_ID, S.LOCATION;";
                var command = new NpgsqlCommand(sourceQuery, sourceConnection);
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var key = reader["Id"];
                    var value1 = reader["Name"];

                    var insertQuery = "INSERT INTO Person (Id, Name) VALUES (@Id, @Name)";
                    var insertCommand = new NpgsqlCommand(insertQuery, destinationConnection);
                    insertCommand.Parameters.AddWithValue("@Id", key);
                    insertCommand.Parameters.AddWithValue("@Name", value1);

                    try
                    {
                        await insertCommand.ExecuteNonQueryAsync();
                        Console.WriteLine($"Inserted record with Id {key}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inserting record with Id {key}: {ex.Message}");
                    }
                }
            }
            else
            {
                // If the destination is not empty, perform updates/inserts based on existence
                var sourceQuery = "SELECT * FROM Items";
                var command = new NpgsqlCommand(sourceQuery, sourceConnection);
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var key = reader["Id"]; // Assuming "Id" is the primary key
                    var value1 = reader["Name"];

                    // Check if the record exists in the destination 'Person' table
                    var checkExistQuery = "SELECT COUNT(*) FROM Person WHERE Id = @Id";
                    var checkCommand = new NpgsqlCommand(checkExistQuery, destinationConnection);
                    checkCommand.Parameters.AddWithValue("@Id", key);

                    var result = await checkCommand.ExecuteScalarAsync();
                    try
                    {
                        var exists = result != null && (long)result > 0;

                        if (exists)
                        {
                            // Update existing record
                            var updateQuery = "UPDATE Person SET Name = @Name WHERE Id = @Id";
                            var updateCommand = new NpgsqlCommand(updateQuery, destinationConnection);
                            updateCommand.Parameters.AddWithValue("@Id", key);
                            updateCommand.Parameters.AddWithValue("@Name", value1);

                            await updateCommand.ExecuteNonQueryAsync();
                            Console.WriteLine($"Updated record with Id {key}.");
                        }
                        else
                        {
                            // Insert new record
                            var insertQuery = "INSERT INTO Person (Id, Name) VALUES (@Id, @Name)";
                            var insertCommand = new NpgsqlCommand(insertQuery, destinationConnection);
                            insertCommand.Parameters.AddWithValue("@Id", key);
                            insertCommand.Parameters.AddWithValue("@Name", value1);

                            await insertCommand.ExecuteNonQueryAsync();
                            Console.WriteLine($"Inserted new record with Id {key}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing record with Id {key}: {ex.Message}");
                    }
                }
            }
        }
    }
}
