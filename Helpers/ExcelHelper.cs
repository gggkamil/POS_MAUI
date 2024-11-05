using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

public class ExcelHelper
{
    public async Task<string> SaveReceiptToExcelAsync(ObservableCollection<ReceiptItem> receiptItems)
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

            worksheet.Cells[1, 1].Value = "Product Name";
            worksheet.Cells[1, 2].Value = "Quantity";
            worksheet.Cells[1, 3].Value = "Unit Price";
            worksheet.Cells[1, 4].Value = "Total Price";
            worksheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;

   
            int row = 2;
            foreach (var item in receiptItems)
            {
                worksheet.Cells[row, 1].Value = item.Name;
                worksheet.Cells[row, 2].Value = item.Quantity;
                worksheet.Cells[row, 3].Value = item.UnitPrice;
                worksheet.Cells[row, 4].Value = item.TotalPrice;
                row++;
            }

          
            worksheet.Cells[row, 3].Value = "Total";
            worksheet.Cells[row, 4].Formula = $"SUM(D2:D{row - 1})";
            worksheet.Cells[row, 3, row, 4].Style.Font.Bold = true;

         
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

          
            var fileBytes = package.GetAsByteArray();
            File.WriteAllBytes(filePath, fileBytes);
        }

        return filePath; 
    }
}
