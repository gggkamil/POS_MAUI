using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

public class ExcelHelper
{
    public async Task<string> SaveReceiptToExcelAsync(ObservableCollection<ReceiptItem> receiptItems, string customerName)
    {
        if (!receiptItems.Any())
        {
            return null;
        }

        string targetDirectory;

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            targetDirectory = Path.Combine(FileSystem.AppDataDirectory, "Receipts");
        }
        else if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            targetDirectory = @"C:\Kasa\Excel";
        }
        else
        {
            targetDirectory = FileSystem.AppDataDirectory;
        }

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        string fileName = $"Rachunek_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        string filePath = Path.Combine(targetDirectory, fileName);
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Rachunek");

            worksheet.Cells[1, 1].Value = "Rachunek";
            worksheet.Cells[2, 1].Value = $"Data: {DateTime.Now:dd-MM-yyyy HH:mm}";
            worksheet.Cells[3, 1].Value = $"Klient: {customerName}";
            worksheet.Cells[1, 1, 3, 1].Style.Font.Bold = true;

            // Column Headers
            worksheet.Cells[5, 1].Value = "Produkt";
            worksheet.Cells[5, 2].Value = "Ilość";
            worksheet.Cells[5, 3].Value = "Cena jednostkowa";
            worksheet.Cells[5, 4].Value = "Cena";
            worksheet.Cells[5, 1, 5, 4].Style.Font.Bold = true;

            // Populate items
            int row = 6;
            foreach (var item in receiptItems)
            {
                worksheet.Cells[row, 1].Value = item.Name;
                worksheet.Cells[row, 2].Value = item.Quantity;
                worksheet.Cells[row, 3].Value = item.UnitPrice;
                worksheet.Cells[row, 4].Value = item.TotalPrice;
                row++;
            }

            // Add total at the bottom
            worksheet.Cells[row, 3].Value = "Suma";
            worksheet.Cells[row, 4].Formula = $"SUM(D6:D{row - 1})";
            worksheet.Cells[row, 3, row, 4].Style.Font.Bold = true;

            // Autofit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Save the file
            var fileBytes = package.GetAsByteArray();
            File.WriteAllBytes(filePath, fileBytes);
        }

        return filePath;
    }
}
