using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ButchersCashier.Models;

namespace ButchersCashier
{
    public static class ProductStorage
    {
        private const string ProductFile = "products.json"; // JSON file to store products

        // Save the products to a JSON file
        public static async Task SaveProductsAsync(List<Product> products) // This can stay public
        {
            var json = JsonSerializer.Serialize(products);
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProductFile);
            await File.WriteAllTextAsync(filePath, json);
        }

        // Load products from a JSON file
        public static async Task<List<Product>> LoadProductsAsync() // This can also stay public
        {
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProductFile);
            if (!File.Exists(filePath))
                return new List<Product>();

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<Product>>(json);
        }
    }
}