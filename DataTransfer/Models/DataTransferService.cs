using DataTransfer.Models;
using Npgsql;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

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
        using (var sourceConnection = new SqlConnection(_source))
        using (var destinationConnection = new NpgsqlConnection(_destination))
        {
            await sourceConnection.OpenAsync();
            await destinationConnection.OpenAsync();

            // Step 1: Fetch records from the source query to get UPCs
            var sourceQuery = @"
                SELECT S.LOCATION, S.EZ_STORE_ID, I.MAINUPC, SI.QUANTITY
                FROM MST_STORE S
                INNER JOIN STK_FACILITY F ON S.STOREID = F.STOREID
                INNER JOIN STK_ITEMSTOCK SI ON F.FACILITYID = SI.FACILITYID
                INNER JOIN ITM_ITEMINFO I ON SI.SKU = I.SKU
                WHERE S.EZ_STORE_ID IS NOT NULL AND S.EZ_STORE_ID > 0 AND F.ISPREDEFINED = 1
                ORDER BY S.EZ_STORE_ID, S.LOCATION;";

            var command = new SqlCommand(sourceQuery, sourceConnection);
            var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var upc = reader["MAINUPC"].ToString();
                var quantity = (decimal)reader["QUANTITY"];

                // Step 2: Get SKU from the UPC in the destination database
                var skuQuery = "SELECT SKU FROM ITM_ITEMUPC WHERE UPC = @Upc LIMIT 1";
                var skuCommand = new NpgsqlCommand(skuQuery, destinationConnection);
                skuCommand.Parameters.AddWithValue("@Upc", upc);

                var skuResult = await skuCommand.ExecuteScalarAsync();

                if (skuResult != null)
                {
                    var sku = (int)skuResult;  // Assuming SKU is an integer

                    // Step 3: Check if POSST_ID exists for the specific SKU and store
                    var posstIdQuery = "SELECT POSST_ID FROM ITM_POSSTOCK WHERE STORE_ID = 14 AND SKU = @Sku";
                    var posstIdCommand = new NpgsqlCommand(posstIdQuery, destinationConnection);
                    posstIdCommand.Parameters.AddWithValue("@Sku", sku);

                    var posstIdResult = await posstIdCommand.ExecuteScalarAsync();

                    if (posstIdResult != null)  // POSST_ID exists, perform update
                    {
                        var updateQuery = "UPDATE ITM_POSSTOCK SET POS_STOCK = @Quantity WHERE STORE_ID = 14 AND SKU = @Sku";
                        var updateCommand = new NpgsqlCommand(updateQuery, destinationConnection);
                        updateCommand.Parameters.AddWithValue("@Quantity", quantity);
                        updateCommand.Parameters.AddWithValue("@Sku", sku);

                        await updateCommand.ExecuteNonQueryAsync();
                    }
                    else  // POSST_ID does not exist, perform insert
                    {
                        var insertQuery = "INSERT INTO ITM_POSSTOCK (STORE_ID, SKU, POS_STOCK) VALUES (14, @Sku, @Quantity)";
                        var insertCommand = new NpgsqlCommand(insertQuery, destinationConnection);
                        insertCommand.Parameters.AddWithValue("@Sku", sku);
                        insertCommand.Parameters.AddWithValue("@Quantity", quantity);

                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    Console.WriteLine($"No SKU found for UPC: {upc}");
                }
            }
        }
    }
}
