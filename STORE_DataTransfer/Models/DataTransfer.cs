using Npgsql;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace STORE_DataTransfer.Models
{
    public class DataTransfer : IDataTransferService
    {
        private readonly string _source;
        private readonly string _destination;

        public DataTransfer(IConfiguration configuration)
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

                var sourceQuery = "SELECT S.LOCATION, S.EZ_STORE_ID, I.MAINUPC, SI.QUANTITY " +
                                  "FROM MST_STORE S " +
                                  "INNER JOIN STK_FACILITY F ON S.STOREID = F.STOREID " +
                                  "INNER JOIN STK_ITEMSTOCK SI ON F.FACILITYID = SI.FACILITYID " +
                                  "INNER JOIN ITM_ITEMINFO I ON SI.SKU = I.SKU " +
                                  "WHERE S.EZ_STORE_ID IS NOT NULL AND S.EZ_STORE_ID > 0 AND F.ISPREDEFINED = 1 " +
                                  "ORDER BY S.EZ_STORE_ID, S.LOCATION;";

                var command = new NpgsqlCommand(sourceQuery, sourceConnection);
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var storeId = reader["EZ_STORE_ID"];
                    var mainUpc = Convert.ToString(reader["MAINUPC"]); // MAINUPC as string
                    var quantity = Convert.ToDecimal(reader["QUANTITY"]);

                    // Retrieve SKU from ITM_VENDORSKU based on MAINUPC
                    var skuQuery = "SELECT SKU FROM ITM_VENDORSKU WHERE VENDORSKU = @Vendorsku";
                    var skuCommand = new NpgsqlCommand(skuQuery, destinationConnection);
                    skuCommand.Parameters.AddWithValue("@Vendorsku",mainUpc); // Assuming mainUpc is being used directly here

                    var skuFromVendor = await skuCommand.ExecuteScalarAsync();

                    if (skuFromVendor == null)
                    {
                        // If not found in ITM_VENDORSKU, check in ITM_ITEMUPC table
                        skuQuery = "SELECT SKU FROM ITM_ITEMUPC WHERE MAINUPC = @MainUpc";
                        skuCommand = new NpgsqlCommand(skuQuery, destinationConnection);
                        skuCommand.Parameters.AddWithValue("@MainUpc", mainUpc);

                        skuFromVendor = await skuCommand.ExecuteScalarAsync();
                    }

                    if (skuFromVendor != null)
                    {
                        var sku = Convert.ToInt64(skuFromVendor);

                        // Check if the store exists in the MST_STORE table
                        var storeCheckQuery = "SELECT STOREID FROM MST_STORE WHERE EZ_STORE_ID = @StoreId";
                        var storeCheckCommand = new NpgsqlCommand(storeCheckQuery, destinationConnection);
                        storeCheckCommand.Parameters.AddWithValue("@StoreId", storeId);

                        var storeExists = await storeCheckCommand.ExecuteScalarAsync();

                        if (storeExists != null)
                        {
                            // Check if the SKU exists in ITM_POSSTOCK
                            var posstQuery = "SELECT POSST_ID FROM ITM_POSSTOCK WHERE STOREID = @StoreId AND SKU = @Sku";
                            var posstCommand = new NpgsqlCommand(posstQuery, destinationConnection);
                            posstCommand.Parameters.AddWithValue("@StoreId", storeId);
                            posstCommand.Parameters.AddWithValue("@Sku", sku);

                            var posstId = await posstCommand.ExecuteScalarAsync();

                            if (posstId != null && (int)posstId > 0)
                            {
                                // Record exists, update POS stock
                                var updateQuery = "UPDATE ITM_POSSTOCK SET POS_STOCK = @Pos_Stock WHERE STOREID = @StoreId AND SKU = @Sku";
                                var updateCommand = new NpgsqlCommand(updateQuery, destinationConnection);
                                updateCommand.Parameters.AddWithValue("@Pos_Stock", quantity);
                                updateCommand.Parameters.AddWithValue("@StoreId", storeId);
                                updateCommand.Parameters.AddWithValue("@Sku", sku);

                                await updateCommand.ExecuteNonQueryAsync();
                                Console.WriteLine($"Updated POS stock for SKU {sku} in store {storeId}.");
                            }
                            else
                            {
                                // Record does not exist, insert a new record
                                var insertQuery = "INSERT INTO ITM_POSSTOCK(STOREID, SKU, POS_STOCK) VALUES(@StoreId, @Sku, @Pos_Stock)";
                                var insertCommand = new NpgsqlCommand(insertQuery, destinationConnection);
                                insertCommand.Parameters.AddWithValue("@StoreId", storeId);
                                insertCommand.Parameters.AddWithValue("@Sku", sku);
                                insertCommand.Parameters.AddWithValue("@Pos_Stock", quantity);

                                await insertCommand.ExecuteNonQueryAsync();
                                Console.WriteLine($"Inserted new POS stock record for SKU {sku} in store {storeId}.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Store with StoreId {storeId} does not exist. No update/insert performed.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"MAINUPC {mainUpc} not found in ITM_VENDORSKU or ITM_ITEMUPC. No update/insert performed.");
                    }
                }
            }
        }
    }
}
