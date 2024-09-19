using Npgsql;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace STORE_DataTransfer.Models
{
    public class DataTransfer : IDataTransferService
    {
        private readonly IConfiguration _configuration;
        private readonly string _source;
        private readonly string _destination;

        public DataTransfer(IConfiguration configuration)
        {
            _configuration = configuration;
            _source = _configuration.GetConnectionString("source");
            _destination = _configuration.GetConnectionString("destination");
        }

        public async Task TransferDataAsync()
        {
            try
            {
                // Retrieve the last process start time from appsettings.json
                var processStartTime = _configuration.GetValue<string>("DataTransferSettings:ProcessStartTime");
                DateTime? lastProcessTime = string.IsNullOrEmpty(processStartTime) ? (DateTime?)null : DateTime.Parse(processStartTime).ToUniversalTime();

                // Update the process start time before starting the data transfer
                UpdateProcessStartTime(DateTime.Now);

                using (var sourceConnection = new SqlConnection(_source))
                using (var destinationConnection = new NpgsqlConnection(_destination))
                {
                    await sourceConnection.OpenAsync();
                    await destinationConnection.OpenAsync();

                    // Define the query based on whether it's the first run or a subsequent run
                    var sourceQuery = lastProcessTime == null ?
                        "SELECT S.LOCATION, S.EZ_STORE_ID, I.MAINUPC, SI.QUANTITY " +
                        "FROM MST_STORE S " +
                        "INNER JOIN STK_FACILITY F ON S.STOREID = F.STOREID " +
                        "INNER JOIN STK_ITEMSTOCK SI ON F.FACILITYID = SI.FACILITYID " +
                        "INNER JOIN ITM_ITEMINFO I ON SI.SKU = I.SKU " +
                        "WHERE S.EZ_STORE_ID IS NOT NULL AND S.EZ_STORE_ID > 0 AND F.ISPREDEFINED = 1 " +
                        "ORDER BY S.EZ_STORE_ID, S.LOCATION;" :
                        "SELECT S.LOCATION, S.EZ_STORE_ID, I.MAINUPC, SI.QUANTITY " +
                        "FROM MST_STORE S " +
                        "INNER JOIN STK_FACILITY F ON S.STOREID = F.STOREID " +
                        "INNER JOIN STK_ITEMSTOCK SI ON F.FACILITYID = SI.FACILITYID " +
                        "INNER JOIN ITM_ITEMINFO I ON SI.SKU = I.SKU " +
                        "WHERE S.EZ_STORE_ID IS NOT NULL AND S.EZ_STORE_ID > 0 AND F.ISPREDEFINED = 1 " +
                        "AND (I.CREATEDATETIME > @ProcessStartTime OR I.UPDATEDATETIME > @ProcessStartTime) " +
                        "ORDER BY S.EZ_STORE_ID, S.LOCATION;";

                    // Load data from the source database into a DataTable
                    var dataTable = new DataTable();
                    using (var command = new SqlCommand(sourceQuery, sourceConnection))
                    {
                        if (lastProcessTime != null)
                        {
                            command.Parameters.Add(new SqlParameter
                            {
                                ParameterName = "@ProcessStartTime",
                                SqlDbType = SqlDbType.DateTimeOffset,
                                Value = lastProcessTime
                            });
                        }

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                    var cmdVendorsku = new NpgsqlCommand();
                    var cmdUPC = new NpgsqlCommand();
                    if(dataTable.Rows.Count > 0)
                    {
                        // Prepare commands for checking SKUs in ITM_VENDORSKU and ITM_ITEMUPC
                        var skuQuery = "SELECT SKU FROM ITM_VENDORSKU WHERE VENDORSKU = @Vendorsku";
                        cmdVendorsku = new NpgsqlCommand(skuQuery, destinationConnection);
                        cmdVendorsku.Parameters.Add("@Vendorsku", NpgsqlTypes.NpgsqlDbType.Text);
                        await cmdVendorsku.PrepareAsync();

                        skuQuery = "SELECT SKU FROM ITM_ITEMUPC WHERE UPC = @UPC";
                         cmdUPC = new NpgsqlCommand(skuQuery, destinationConnection);
                        cmdUPC.Parameters.Add("@UPC", NpgsqlTypes.NpgsqlDbType.Text);
                        await cmdUPC.PrepareAsync();
                    }
                    

                    // Process each row and perform insert/update operations
                    foreach (DataRow row in dataTable.Rows)
                    {
                        try
                        {
                            var storeId = row["EZ_STORE_ID"] ?? DBNull.Value;
                            var mainUpc = row["MAINUPC"] != DBNull.Value ? Convert.ToString(row["MAINUPC"]) : string.Empty;
                            var quantity = row["QUANTITY"] != DBNull.Value ? Convert.ToDecimal(row["QUANTITY"]) : 0;

                            // Check if SKU exists in ITM_VENDORSKU or ITM_ITEMUPC
                            cmdVendorsku.Parameters["@Vendorsku"].Value = mainUpc;
                            var skuFromVendor = await cmdVendorsku.ExecuteScalarAsync();

                            if (skuFromVendor == null)
                            {
                                cmdUPC.Parameters["@UPC"].Value = mainUpc;
                                skuFromVendor = await cmdUPC.ExecuteScalarAsync();
                            }

                            if (skuFromVendor != null)
                            {
                                var sku = Convert.ToInt64(skuFromVendor);

                                // Check if the store exists in MST_STORE
                                var storeCheckQuery = "SELECT STOREID FROM MST_STORE WHERE EZ_STORE_ID = @StoreId";
                                using (var storeCheckCommand = new SqlCommand(storeCheckQuery, sourceConnection))
                                {
                                    storeCheckCommand.Parameters.AddWithValue("@StoreId", storeId);
                                    var storeExists = await storeCheckCommand.ExecuteScalarAsync();

                                    if (storeExists != null)
                                    {
                                        // Check if SKU exists in ITM_POSSTOCK
                                        var posstQuery = "SELECT POSST_ID FROM ITM_POSSTOCK WHERE STORE_ID = @StoreId AND SKU = @Sku";
                                        using (var posstCommand = new NpgsqlCommand(posstQuery, destinationConnection))
                                        {
                                            posstCommand.Parameters.AddWithValue("@StoreId", storeId);
                                            posstCommand.Parameters.AddWithValue("@Sku", sku);
                                            var posstId = await posstCommand.ExecuteScalarAsync();

                                            if (posstId != null && (int)posstId > 0)
                                            {
                                                try
                                                {
                                                    // Update POS stock
                                                    var updateQuery = "UPDATE ITM_POSSTOCK SET POS_STOCK = @Pos_Stock WHERE STORE_ID = @StoreId AND SKU = @Sku";
                                                    using (var updateCommand = new NpgsqlCommand(updateQuery, destinationConnection))
                                                    {
                                                        updateCommand.Parameters.AddWithValue("@Pos_Stock", quantity);
                                                        updateCommand.Parameters.AddWithValue("@StoreId", storeId);
                                                        updateCommand.Parameters.AddWithValue("@Sku", sku);
                                                        await updateCommand.ExecuteNonQueryAsync();
                                                        Console.WriteLine($"Updated POS stock for SKU {sku} in store {storeId}.");
                                                    }
                                                }
                                                catch(Exception ex)
                                                {
                                                    Console.WriteLine($"Error for Updating Records : {ex.Message}");
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    // Insert new record in ITM_POSSTOCK
                                                    var insertQuery = "INSERT INTO ITM_POSSTOCK(STORE_ID, SKU, POS_STOCK) VALUES(@StoreId, @Sku, @Pos_Stock)";
                                                    using (var insertCommand = new NpgsqlCommand(insertQuery, destinationConnection))
                                                    {
                                                        insertCommand.Parameters.AddWithValue("@StoreId", storeId);
                                                        insertCommand.Parameters.AddWithValue("@Sku", sku);
                                                        insertCommand.Parameters.AddWithValue("@Pos_Stock", quantity);
                                                        await insertCommand.ExecuteNonQueryAsync();
                                                        Console.WriteLine($"Inserted new POS stock record for SKU {sku} in store {storeId}.");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"Error for Inserting Records : {ex.Message}");
                                                    continue;
                                                }
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
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during data transfer: {ex.Message}");
                
            }
        }

        // Method to update process start time in appsettings.json
        private void UpdateProcessStartTime(DateTime newProcessTime)
        {
            var filePath = "appsettings.json";
            try
            {
                var json = File.ReadAllText(filePath);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);
                jsonObj["DataTransferSettings"]["ProcessStartTime"] = newProcessTime.ToString("yyyy-MM-ddTHH:mm:ss");

                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(filePath, output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating process start time: {ex.Message}");
            }
        }
    }
}
