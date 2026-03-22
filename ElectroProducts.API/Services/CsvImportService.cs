using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ElectroProducts.API.Extensions;
using ElectroProducts.Models.Models;

namespace ElectroProducts.API.Services
{
    public class CsvImportService
    {
        /// <summary>
        /// Imports data from a CSV file, maps it to a domain model, and returns a DataTable along with the count of records.
        /// </summary>
        /// <typeparam name="TDto">The type of the Data Transfer Object (DTO) representing the CSV data.</typeparam>
        /// <typeparam name="TDomain">The type of the domain model.</typeparam>
        /// <typeparam name="TMap">The type of the CSV mapping class.</typeparam>
        /// <param name="filePath">The path to the CSV file.</param>
        /// <param name="mapper">A function to map the DTO to the domain model.</param>
        /// <param name="dtoFilter">An optional filter function to filter the DTOs before mapping.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A tuple containing a DataTable with the mapped data and the count of records.</returns>
        public async Task<(DataTable Table, int Count)> ImportDataFromCsvFile<TDto, TDomain, TMap>(
            string filePath,
            Func<TDto, TDomain> mapper,
            Func<TDto, bool>? dtoFilter = null,
            CancellationToken cancellationToken = default)
            where TDto : class
            where TDomain : class, IHasSku
            where TMap : ClassMap<TDto>
        {
            // Configure CsvHelper to read the CSV file with appropriate settings
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                DetectDelimiter = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                ShouldSkipRecord = args => args.Row.Parser.Record!.All(string.IsNullOrWhiteSpace)
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<TMap>();

            // Read data from CSV and map to DTOs
            var records = csv.GetRecords<TDto>();

            // Filter data at the DTO level if a filter function is provided
            if (dtoFilter != null)
                records = records.Where(dtoFilter);

            // Map data to the domain model - filter out items with empty SKU and remove duplicates by SKU
            // The IHasSku interface informs the compiler that TDomain has a SKU property, allowing safe filtering and grouping of data
            var list = records
                .Select(mapper)
                .Where(r => !string.IsNullOrWhiteSpace(r.SKU))
                .GroupBy(r => r.SKU)
                .Select(g => g.First())
                .ToList();

            return (list.ToDataTable(), list.Count);
        }
    }
}
