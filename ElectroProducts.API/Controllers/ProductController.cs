using ElectroProducts.API.Services;
using ElectroProducts.Models.Domains;
using Microsoft.AspNetCore.Mvc;

namespace ElectroProducts.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Tags("Products")]
    public class ProductController : ControllerBase
    {

        private readonly ILogger<ProductController> _logger;
        private readonly FileDownloadService _fileDownloadService;
        private readonly DbService _dbService;

        public ProductController(ILogger<ProductController> logger, FileDownloadService fileDownloadService, DbService dbService)
        {
            _logger = logger;
            _fileDownloadService = fileDownloadService;
            _dbService = dbService;
        }


        /// <summary>
        /// Downloads CSV files from configured URLs, imports the data into the database within a transaction, and returns the count of records inserted for each data type. If any step fails, an appropriate error message is returned, and all changes are rolled back to maintain data integrity. Temporary files are cleaned up after the operation, regardless of success or failure.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A dictionary containing the count of records inserted for 'Products', 'Inventories', and 'Prices'.</returns>
        [HttpPost("populate")]
        [EndpointSummary("Pobierz pliki CSV i wypełnij bazę danych")]
        [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PopulateProductsDB(CancellationToken cancellationToken)
        {

            // Realisation steps:
            // 1. Download files to temp location
            string[] files;
            try
            {
                files = await _fileDownloadService.DownloadAllFilesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas pobierania plików: {ex.Message}");
            }

            // 2. Import data from files to DB within transaction, return count of inserted records for each type, if any error occurs return appropriate message and rollback all changes
            try
            {
                var results = await _dbService.InsertData(files, cancellationToken);
                return Ok(results);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas importu, zmiany cofnięte: {ex.Message}");
            }
            finally
            {
                // 3. Always clean up temp files
                foreach (var file in files)
                {
                    if (System.IO.File.Exists(file))
                        System.IO.File.Delete(file);
                }
            }
        }

        /// <summary>
        /// Retrieves the details of a product identified by its SKU (Stock Keeping Unit).
        /// </summary>
        /// <remarks>If the specified SKU does not correspond to an existing product, the method returns a
        /// 404 Not Found response. In the event of an internal error, a 500 Internal Server Error response is returned
        /// with an error message.</remarks>
        /// <param name="sku">The SKU of the product to retrieve. Must be a non-null, non-empty string representing a valid product SKU.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An ActionResult containing the product details if found; otherwise, a 404 Not Found response if the product
        /// does not exist, or a 500 Internal Server Error response if an unexpected error occurs.</returns>
        [HttpGet("{sku}")]
        [EndpointSummary("Pobierz produkt po SKU")]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductResponse>> GetProductBySku(string sku, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(sku))
                    return BadRequest("SKU nie może być puste.");

                var product = await _dbService.GetProductBySkuAsync(sku, cancellationToken);
                if (product == null)
                    return NotFound($"Produkt o SKU '{sku}' nie istnieje.");

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas pobierania produktu: {ex.Message}");
            }
        }
    }
}
