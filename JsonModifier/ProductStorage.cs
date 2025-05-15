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
                    db.Products.Add(product); // New product
                }
                else
                {
                    var existing = await db.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
                    if (existing != null)
                    {
                        db.Entry(existing).CurrentValues.SetValues(product); // Update
                    }
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
