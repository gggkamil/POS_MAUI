using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

public class ExcelHelper
{
    public async Task<string> SaveReceiptToExcelAsync(IEnumerable<ReceiptItem> receiptItems)
    {
        if (!receiptItems.Any())
        {
            return null; // Return null if there are no items
        }

        // Define the target directory based on the platform
        string targetDirectory;

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            // On Android, save to the Downloads folder
            targetDirectory = Path.Combine(FileSystem.AppDataDirectory, "Receipts");
        }
        else if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            // On Windows, save to C:\Documents\Excel
            targetDirectory = @"C:\Users\kamil\Documents\Excel";
        }
        else
        {
            // Fallback to the app's data directory for other platforms
            targetDirectory = FileSystem.AppDataDirectory;
        }

        // Ensure the directory exists
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        // Define file name and path
        string fileName = $"Receipt_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        string filePath = Path.Combine(targetDirectory, fileName);
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        // Create an Excel file
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Receipt");

            // Add headers
            worksheet.Cells[1, 1].Value = "Product Name";
            worksheet.Cells[1, 2].Value = "Quantity";
            worksheet.Cells[1, 3].Value = "Unit Price";
            worksheet.Cells[1, 4].Value = "Total Price";
            worksheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;

            // Populate data from ReceiptItems
            int row = 2;
            foreach (var item in receiptItems)
            {
                worksheet.Cells[row, 1].Value = item.Name;
                worksheet.Cells[row, 2].Value = item.Quantity;
                worksheet.Cells[row, 3].Value = item.UnitPrice;
                worksheet.Cells[row, 4].Value = item.TotalPrice;
                row++;
            }

            // Add total sum at the end
            worksheet.Cells[row, 3].Value = "Total";
            worksheet.Cells[row, 4].Formula = $"SUM(D2:D{row - 1})";
            worksheet.Cells[row, 3, row, 4].Style.Font.Bold = true;

            // Adjust column widths
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Save to file
            var fileBytes = package.GetAsByteArray();
            File.WriteAllBytes(filePath, fileBytes);
        }

        return filePath; // Return the file path of the saved file
    }
}
