using ElectroProducts.Models.Models;

namespace ElectroProducts.API.Services
{
    public class FileDownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public FileDownloadService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves the URLs and save paths for the CSV files from the configuration. It constructs an array of CsvFileInfo objects containing this information.
        /// </summary>
        /// <returns>An array of <see cref="CsvFileInfo"/> objects containing the URLs and save paths for the CSV files.</returns>
        /// <exception cref="InvalidOperationException">Thrown if any of the required URLs are missing in the configuration.</exception>
        private async Task<CsvFileInfo[]> GetCsvFilesData()
        {
            var tempPath = _configuration["TempPath"] ?? Path.GetTempPath();

            var productsUrl = _configuration["SourceFiles:Products"] ?? throw new InvalidOperationException("Brak URL dla Products");
            var inventoryUrl = _configuration["SourceFiles:Inventory"] ?? throw new InvalidOperationException("Brak URL dla Inventory");
            var priceUrl = _configuration["SourceFiles:Price"] ?? throw new InvalidOperationException("Brak URL dla Price");

            return new CsvFileInfo[]
            {
                new(productsUrl,  Path.Combine(tempPath, "Products.csv")),
                new(inventoryUrl, Path.Combine(tempPath, "Inventory.csv")),
                new(priceUrl,     Path.Combine(tempPath, "Price.csv")),
            };
        }

        /// <summary>
        /// Asynchronously downloads the content from the specified URL and saves it to the specified file path.
        /// </summary>
        /// <remarks>If the download fails, an exception will be thrown. Ensure that the URL is valid and
        /// that the application has permission to write to the specified path.</remarks>
        /// <param name="url">The URL from which to download the content. This must be a valid, accessible URL.</param>
        /// <param name="savePath">The file path where the downloaded content will be saved. This path must be writable.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The file path where the content has been saved.</returns>
        private async Task<string> DownloadAsync(string url, string savePath, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var fs = new FileStream(savePath, FileMode.Create);
            await response.Content.CopyToAsync(fs, cancellationToken);
            return savePath;
        }

        /// <summary>
        /// Downloads a file from the specified URL and saves it to the given path, retrying the operation if it fails.
        /// </summary>
        /// <remarks>This method will attempt to download the file up to the specified number of retries,
        /// waiting for the specified delay between attempts.</remarks>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="savePath">The local file path where the downloaded file will be saved.</param>
        /// <param name="maxRetries">The maximum number of retry attempts if the download fails. Defaults to 3.</param>
        /// <param name="delayMilliseconds">The delay, in milliseconds, between retry attempts. Defaults to 2000.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The path to the downloaded file upon successful completion.</returns>
        /// <exception cref="Exception">Thrown if the download fails after the maximum number of retry attempts.</exception>
        private async Task<string> DownloadWithRetriesAsync(string url, string savePath, int maxRetries = 3, int delayMilliseconds = 2000, CancellationToken cancellationToken = default)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await DownloadAsync(url, savePath, cancellationToken);
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    Console.WriteLine($"Attempt {attempt} failed: {ex.Message}. Retrying in {delayMilliseconds}ms...");
                    await Task.Delay(delayMilliseconds);
                }
            }
            throw new Exception($"Failed to download file from {url} after {maxRetries} attempts.");
        }

        /// <summary>
        /// Asynchronously downloads all CSV files from their specified URLs and saves them to the designated file
        /// paths.
        /// </summary>
        /// <remarks>This method retrieves a list of CSV file metadata, initiates parallel download
        /// operations for each file, and waits for all downloads to complete. Callers should handle exceptions that may
        /// occur during the download process, such as network errors or file access issues.</remarks>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A string array containing the file paths of all successfully downloaded files.</returns>
        public async Task<string[]> DownloadAllFilesAsync(CancellationToken cancellationToken = default)
        {
            var files = await GetCsvFilesData();
            var downloadTasks = files.Select(f => DownloadWithRetriesAsync(f.Url, f.SavePath, cancellationToken: cancellationToken)).ToArray();
            return await Task.WhenAll(downloadTasks);
        }
    }
}
