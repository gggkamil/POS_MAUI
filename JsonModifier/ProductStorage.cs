using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ButchersCashier.Models;
using ButchersCashier.Data;
using Microsoft.EntityFrameworkCore;

namespace ButchersCashier
{
    public static class ProductStorage
    {
        public static async Task<List<Product>> LoadProductsAsync()
        {
            using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
            return await db.Products.OrderBy(p => p.Id).ToListAsync();
        }

        public static async Task SaveProductsAsync(List<Product> products)
        {
            using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());

            foreach (var product in products)
            {
                if (product.Id == 0)
                {
                    db.Products.Add(product); // Let SQL generate Id
                }
                else
                {
                    var existing = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);
                    if (existing != null)
                    {
                        db.Products.Update(product);
                    }
                    else
                    {
                        // NEW product has manually set Id → RESET it
                        product.Id = 0;
                        db.Products.Add(product); // Insert without forcing Id
                    }
                }
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var root = ex.GetBaseException();
                string message = $"DB Save Error:\n{root.GetType().Name}\n{root.Message}";

                System.Diagnostics.Debug.WriteLine(message);
                await Application.Current.MainPage.DisplayAlert("Błąd zapisu do bazy", message, "OK");
            }
        }


    }
}
