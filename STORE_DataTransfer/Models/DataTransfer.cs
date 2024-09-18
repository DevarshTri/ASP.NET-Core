using Npgsql;
using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

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
            try
            {
                using (var sourceConnection = new SqlConnection(_source))
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

                    // Load data from source database into a DataTable
                    var dataTable = new DataTable();

                    using (var command = new SqlCommand(sourceQuery, sourceConnection))
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }

                    NpgsqlCommand cmd_Vendorsku = new NpgsqlCommand();
                    NpgsqlCommand cmd_UPC = new NpgsqlCommand();

                    if (dataTable.Rows.Count > 0)
                    {
                        var skuQuery = "SELECT SKU FROM ITM_VENDORSKU WHERE VENDORSKU = @Vendorsku";
                        cmd_Vendorsku = new NpgsqlCommand(skuQuery, destinationConnection);
                        cmd_Vendorsku.Parameters.Add("@Vendorsku", NpgsqlTypes.NpgsqlDbType.Text);
                        await cmd_Vendorsku.PrepareAsync();

                        skuQuery = "SELECT SKU FROM ITM_ITEMUPC WHERE UPC = @UPC";
                        cmd_UPC = new NpgsqlCommand(skuQuery, destinationConnection);
                        cmd_UPC.Parameters.Add("@UPC", NpgsqlTypes.NpgsqlDbType.Text);
                        await cmd_UPC.PrepareAsync();
                    }

                    foreach (DataRow row in dataTable.Rows)
                    {
                        try
                        {
                            var storeId = row["EZ_STORE_ID"];
                            var mainUpc = Convert.ToString(row["MAINUPC"]);
                            var quantity = Convert.ToDecimal(row["QUANTITY"]);

                            // Create a new command and execute it to get SKU
                            cmd_Vendorsku.Parameters["@Vendorsku"].Value = mainUpc;
                            var skuFromVendor = await cmd_Vendorsku.ExecuteScalarAsync();

                            if (skuFromVendor == null)
                            {
                                // Check in ITM_ITEMUPC table if not found in ITM_VENDORSKU
                                cmd_UPC.Parameters["@UPC"].Value = mainUpc;
                                skuFromVendor = await cmd_UPC.ExecuteScalarAsync();
                            }

                            if (skuFromVendor != null)
                            {
                                var sku = Convert.ToInt64(skuFromVendor);

                                // Check if the store exists in the MST_STORE table
                                var storeCheckQuery = "SELECT STOREID FROM MST_STORE WHERE EZ_STORE_ID = " + storeId + "";
                                using (var storeCheckCommand = new SqlCommand(storeCheckQuery, sourceConnection))
                                {

                                    var storeExists = await storeCheckCommand.ExecuteScalarAsync();

                                    if (storeExists != null)
                                    {
                                        // Check if the SKU exists in ITM_POSSTOCK
                                        var posstQuery = "SELECT POSST_ID FROM ITM_POSSTOCK WHERE STORE_ID =" + storeId + " AND SKU = " + sku + "";
                                        var posstCommand = new NpgsqlCommand(posstQuery, destinationConnection);

                                        var posstId = await posstCommand.ExecuteScalarAsync();

                                        if (posstId != null && (int)posstId > 0)
                                        {
                                            try
                                            {
                                                // Update POS stock
                                                var updateQuery = "UPDATE ITM_POSSTOCK SET POS_STOCK = @Pos_Stock WHERE STORE_ID = @StoreId AND SKU = @Sku";
                                                var updateCommand = new NpgsqlCommand(updateQuery, destinationConnection);

                                                updateCommand.Parameters.AddWithValue("@Pos_Stock", quantity);
                                                updateCommand.Parameters.AddWithValue("@StoreId", storeId);
                                                updateCommand.Parameters.AddWithValue("@Sku", sku);
                                                await updateCommand.ExecuteNonQueryAsync();
                                                Console.WriteLine($"Updated POS stock for SKU {sku} in store {storeId}.");

                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"Error to Update records {ex.Message}");
                                            }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                // Insert new record
                                                var insertQuery = "INSERT INTO ITM_POSSTOCK(STORE_ID, SKU, POS_STOCK) VALUES(@StoreId, @Sku, @Pos_Stock)";
                                                var insertCommand = new NpgsqlCommand(insertQuery, destinationConnection);

                                                insertCommand.Parameters.AddWithValue("@StoreId", storeId);
                                                insertCommand.Parameters.AddWithValue("@Sku", sku);
                                                insertCommand.Parameters.AddWithValue("@Pos_Stock", quantity);
                                                await insertCommand.ExecuteNonQueryAsync();
                                                Console.WriteLine($"Inserted new POS stock record for SKU {sku} in store {storeId}.");

                                            }
                                            catch (Exception ex)
                                            
                                            {
                                                Console.WriteLine($"Error to insert records {ex.Message}");
                                            }
                                        }

                                    }
                                    else
                                    {
                                        Console.WriteLine($"Store with StoreId {storeId} does not exist. No update/insert performed.");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"MAINUPC {mainUpc} not found in ITM_VENDORSKU or ITM_ITEMUPC. No update/insert performed.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing row with MAINUPC {row["MAINUPC"]}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during data transfer: {ex.Message}");
            }
        }
    }
}
