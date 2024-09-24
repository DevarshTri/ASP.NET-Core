using Npgsql;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data.SqlClient;
using log4net;
using System.ComponentModel;

namespace STORE_DataTransfer.Models
{
    public class DataTransfer : IDataTransferService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DataTransfer));

        private readonly IConfiguration _configuration;

        // private readonly string _source;
        private readonly string _destination;
        private readonly List<string> _lstSource;

        public DataTransfer(IConfiguration configuration)
        {
            _configuration = configuration;

            _lstSource = new List<string>();
            _lstSource.Add(_configuration.GetConnectionString("source"));
            _lstSource.Add(_configuration.GetConnectionString("source_1"));

            _destination = _configuration.GetConnectionString("destination");
        }

        public async Task TransferDataAsync()
        {
            log.Info("Starting data transfer process.");

            try
            {
                // Retrieve the last process start time from appsettings.json
                var processStartTime = _configuration.GetValue<string>("DataTransferSettings:ProcessStartTime");
                DateTime? lastProcessTime = string.IsNullOrEmpty(processStartTime) ? (DateTime?)null : DateTime.Parse(processStartTime).ToUniversalTime();

                // Update the process start time before starting the data transfer
                UpdateProcessStartTime(DateTime.Now);
                log.Info($"Process start time:{DateTime.Now}");

                using (NpgsqlConnection destinationConnection = new NpgsqlConnection(_destination))
                {
                    await destinationConnection.OpenAsync();

                    foreach (var source in _lstSource)
                    {
                        bool bDBType_SQL;
                        if (source.ToUpper().Contains("AKSHARPITH_BAPSINDIA"))
                            bDBType_SQL = true;
                        else
                            bDBType_SQL = false;

                        using (var sourceConnection_SQL = bDBType_SQL ? new SqlConnection(source) : null)
                        using (var sourceConnection_PG = bDBType_SQL ? null : new NpgsqlConnection(source))
                        {
                            if (bDBType_SQL)
                            {
                                await sourceConnection_SQL.OpenAsync();
                                log.Info("SQL Connection Open");
                            }
                            else
                            {
                                await sourceConnection_PG.OpenAsync();
                                log.Info("PG Connection Open");
                            }

                            log.Info("Connected to source and destination databases.");

                            var sourceQuery = "SELECT S.LOCATION, S.EZ_STORE_ID, I.MAINUPC, SI.QUANTITY " +
                                "FROM MST_STORE S " +
                                "INNER JOIN STK_FACILITY F ON S.STOREID = F.STOREID " +
                                "INNER JOIN STK_ITEMSTOCK SI ON F.FACILITYID = SI.FACILITYID " +
                                "INNER JOIN ITM_ITEMINFO I ON SI.SKU = I.SKU " +
                                "WHERE S.EZ_STORE_ID IS NOT NULL AND S.EZ_STORE_ID > 0 AND F.ISPREDEFINED = 1 ";

                            if (lastProcessTime != null)
                            {
                                DateTime _lastProcessTime = (DateTime)lastProcessTime; // Convert.ToDateTime(lastProcessTime);
                                sourceQuery += " AND (I.CREATEDATETIME > '" + _lastProcessTime.ToString("yyyy-MM-dd HH:mm:ss") + "' OR I.UPDATEDATETIME > '" + _lastProcessTime.ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                            }

                            sourceQuery += " ORDER BY S.EZ_STORE_ID, S.LOCATION;";

                            // Load data from the source database into a DataTable
                            var dataTable = new DataTable();

                            if (bDBType_SQL)
                            {
                                using (var adapter = new SqlDataAdapter(sourceQuery, sourceConnection_SQL))
                                {
                                    adapter.Fill(dataTable);
                                    log.Info($"Source query executed. Number of records fetched: {dataTable.Rows.Count}");
                                }
                            }
                            else
                            {
                                using (var adapter = new NpgsqlDataAdapter(sourceQuery, sourceConnection_PG))
                                {
                                    adapter.Fill(dataTable);
                                    log.Info($"Source query executed. Number of records fetched: {dataTable.Rows.Count}");
                                }
                            }

                            if (dataTable.Rows.Count == 0)
                            {
                                log.Warn("No records found for transfer.");
                                return;
                            }

                            log.Warn("Total records found for transfer : " + dataTable.Rows.Count);

                            var cmdVendorsku = new NpgsqlCommand();
                            var cmdUPC = new NpgsqlCommand();
                            if (dataTable.Rows.Count > 0)
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

                                        // Check if SKU exists in ITM_POSSTOCK
                                        var posstQuery = "SELECT POSST_ID FROM ITM_POSSTOCK WHERE STORE_ID = " + storeId + " AND SKU = " + sku;
                                        using (var posstCommand = new NpgsqlCommand(posstQuery, destinationConnection))
                                        {
                                            log.Debug("ITM_POSSTOCK.SELECT.posstQuery : " + posstQuery);

                                            var posstId = await posstCommand.ExecuteScalarAsync();

                                            if (posstId != null && (int)posstId > 0)
                                            {
                                                try
                                                {
                                                    // Update POS stock
                                                    var updateQuery = "UPDATE ITM_POSSTOCK SET POS_STOCK = " + quantity + " WHERE STORE_ID = " + storeId + " AND SKU = " + sku;
                                                    using (var updateCommand = new NpgsqlCommand(updateQuery, destinationConnection))
                                                    {
                                                        log.Debug("ITM_POSSTOCK.UPDATE.updateQuery  : " + updateQuery);

                                                        await updateCommand.ExecuteNonQueryAsync();

                                                        log.Info($"Updated POS stock for SKU {sku} in store {storeId}.");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    log.Error($"Error for Updating Records : {ex.Message}");
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    // Insert new record in ITM_POSSTOCK
                                                    var insertQuery = "INSERT INTO ITM_POSSTOCK(STORE_ID, SKU, POS_STOCK) VALUES(" + storeId + ", " + sku + ", " + quantity + ")";
                                                    using (var insertCommand = new NpgsqlCommand(insertQuery, destinationConnection))
                                                    {
                                                        log.Debug("ITM_POSSTOCK.INSERT.insertQuery  : " + insertQuery);

                                                        await insertCommand.ExecuteNonQueryAsync();

                                                        log.Info($"Inserted new POS stock record for SKU {sku} in store {storeId}.");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    log.Error($"Error for Inserting Records : {ex.Message}");
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        log.Warn($"MAINUPC {mainUpc} not found in ITM_VENDORSKU or ITM_ITEMUPC. No update/insert performed.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error($"Error processing row with MAINUPC {row["MAINUPC"]}: {ex.Message}");
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"An error occurred during data transfer: {ex.Message}");

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
                log.Info($"Error updating process start time: {ex.Message}");
            }
        }
    }
}
