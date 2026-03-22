using System.Data;
using System.Reflection;

namespace ElectroProducts.API.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Extension method to convert a list of objects to a DataTable. This is useful for bulk operations or when working with data that needs to be represented in a tabular format.
        /// </summary>
        /// <typeparam name="T">The type of objects in the list.</typeparam>
        /// <param name="items">The list of objects to convert.</param>
        /// <returns>A DataTable representing the list of objects.</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> items)
        {
            var table = new DataTable();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (var item in items)
            {
                var row = table.NewRow();
                foreach (var prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
