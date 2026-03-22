using System.Data;
using Dapper;
using ElectroProducts.Models.Domains;
using ElectroProducts.Models.Mappers;
using Microsoft.Data.SqlClient;

namespace ElectroProducts.API.Services
{
    public class DbService
    {
        private string _connectionString { get; set; } = string.Empty;
        private readonly CsvImportService _csvImportService;
        private readonly IConfiguration _configuration;

        public DbService(IConfiguration configuration, CsvImportService csvImportService)
        {
            _csvImportService = csvImportService;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }

        /// <summary>
        /// Inserts data from the specified CSV files into the database, returning the count of records inserted for
        /// each data type.
        /// </summary>
        /// <remarks>This method requires that the CSV files are formatted correctly and that the database
        /// connection string is valid. It performs a bulk insert operation after clearing existing data in the relevant
        /// tables.</remarks>
        /// <param name="files">An array of strings representing the file paths of the CSV files to be imported. The first file should
        /// contain product data, the second should contain inventory data, and the third should contain price data.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A dictionary containing the count of records inserted for 'Products', 'Inventories', and 'Prices'.</returns>
        /// <exception cref="Exception">Thrown if an error occurs during the import process, indicating that the transaction has been rolled back.</exception>
        public async Task<Dictionary<string, int>> InsertData(string[] files, CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();
            var results = new Dictionary<string, int>();

            try
            {
                // Import data from CSV files into DataTables
                var (productsTable, productsCount) = await _csvImportService.ImportDataFromCsvFile<ProductDTO, Product, ProductMap>(files[0], dto => dto.ToProduct(), dtoFilter: dto => dto.IsWire == false && dto.Shipping == "24h", cancellationToken: cancellationToken);
                var (inventoriesTable, inventoriesCount) = await _csvImportService.ImportDataFromCsvFile<InventoryDTO, Inventory, InventoryMap>(files[1], dto => dto.ToInventory(), dtoFilter: dto => dto.Shipping == "24h", cancellationToken: cancellationToken);
                var (pricesTable, pricesCount) = await _csvImportService.ImportDataFromCsvFile<Price, Price, PriceMap>(files[2], dto => dto, cancellationToken: cancellationToken);

                // Clear tables
                await connection.ExecuteAsync(new CommandDefinition("DELETE FROM Inventories", cancellationToken: cancellationToken, transaction: transaction));
                await connection.ExecuteAsync(new CommandDefinition("DELETE FROM Prices", cancellationToken: cancellationToken, transaction: transaction));
                await connection.ExecuteAsync(new CommandDefinition("DELETE FROM Products", cancellationToken: cancellationToken, transaction: transaction));

                // Bulk insert data into tables
                await BulkInsertAsync(productsTable, "Products", connection, transaction, cancellationToken);
                await BulkInsertAsync(inventoriesTable, "Inventories", connection, transaction, cancellationToken);
                await BulkInsertAsync(pricesTable, "Prices", connection, transaction, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                results["Products"] = productsCount;
                results["Inventories"] = inventoriesCount;
                results["Prices"] = pricesCount;

                return results;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new Exception($"Błąd podczas importu, zmiany cofnięte: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously inserts a bulk set of rows from the specified DataTable into the designated SQL Server table
        /// within the context of a transaction.
        /// </summary>
        /// <remarks>The bulk insert is performed in batches of 5,000 rows, with a timeout of 120 seconds
        /// per batch.</remarks>
        /// <param name="table">The DataTable containing the rows to insert. The schema of the DataTable must match the schema of the
        /// destination table.</param>
        /// <param name="tableName">The name of the destination table in the SQL Server database where the data will be inserted.</param>
        /// <param name="connection">An open SqlConnection to the SQL Server database where the bulk insert operation will be performed.</param>
        /// <param name="transaction">The SqlTransaction under which the bulk insert operation will execute. The operation is committed or rolled
        /// back with this transaction.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous bulk insert operation.</returns>
        private static async Task BulkInsertAsync(DataTable table, string tableName, SqlConnection connection, SqlTransaction transaction, CancellationToken cancellationToken = default)
        {
            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                DestinationTableName = tableName,
                BatchSize = 5000,
                BulkCopyTimeout = 120
            };

            await bulkCopy.WriteToServerAsync(table, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves product details for the specified SKU.
        /// </summary>
        /// <param name="sku">The unique stock keeping unit (SKU) of the product to retrieve. Cannot be null or empty.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ProductResponse"/> containing the product information if found; otherwise, <see
        /// langword="null"/>.</returns>
        public async Task<ProductResponse?> GetProductBySkuAsync(string sku, CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<ProductResponse>(
                new CommandDefinition(
                    @"SELECT 
                        p.SKU,
                        p.Name,
                        p.EAN,
                        p.ProducerName,
                        p.Category,
                        p.PhotoUrl,
                        i.StockQuantity,
                        i.LogisticUnit,
                        pr.LogisticUnitPrice,
                        i.ShippingCost AS DeliveryCost
                      FROM Products p
                      LEFT JOIN Inventories i  ON i.SKU  = p.SKU
                      LEFT JOIN Prices      pr ON pr.SKU = p.SKU
                      WHERE p.SKU = @SKU",
                     parameters: new { SKU = sku }, cancellationToken: cancellationToken)
            );
        }
    }
}
