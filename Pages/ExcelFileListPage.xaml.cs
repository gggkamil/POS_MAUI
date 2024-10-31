using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using ButchersCashier.Models;
using Microsoft.Maui.Controls;
using OfficeOpenXml; // Ensure you have the EPPlus library for Excel manipulation
using System.Threading.Tasks;

namespace CashierApps
{
    public partial class ExcelFileListPage : ContentPage
    {
        public ObservableCollection<ExcelFile> ExcelFiles { get; set; } = new();

        // Reference to the receipt items from ProductsPage
        public ObservableCollection<ReceiptItem> ReceiptItems { get; private set; }

        public ExcelFileListPage(ObservableCollection<ReceiptItem> receiptItems) // Constructor with parameter
        {
            InitializeComponent();
            BindingContext = this;
            ReceiptItems = receiptItems; // Assign the passed receipt items
            LoadExcelFiles(); // Load existing Excel files
        }

        private void LoadExcelFiles()
        {
            var folderPath = @"C:\Documents\WZ"; // Directory to load files from
            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath, "*.xlsx");
                foreach (var file in files)
                {
                    ExcelFiles.Add(new ExcelFile
                    {
                        FileName = Path.GetFileName(file),
                        FilePath = file,
                        IsSelected = false // Initialize selection state
                    });
                }
            }
        }

        public string GetSelectedFilePath()
        {
            // Return the path of the selected file, if any
            var selectedFile = ExcelFiles.FirstOrDefault(f => f.IsSelected);
            return selectedFile?.FilePath; // Return the path if a file is selected
        }

        // Method to save ReceiptItems to the selected Excel file
        public async Task SaveReceiptItemsToSelectedFileAsync()
        {
            var selectedFilePath = GetSelectedFilePath(); // Get the selected file path

            if (string.IsNullOrEmpty(selectedFilePath))
            {
                return;
            }
            if (ReceiptItems.Count == 0)
            {
                await DisplayAlert("Warning", "No items to save!", "OK");
                return;
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(selectedFilePath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault() ?? package.Workbook.Worksheets.Add("Receipts");

                    // Determine the starting row for new data entries
                    int startRow = worksheet.Dimension?.Rows + 1 ?? 1;

                    // If starting row is 1, this means the worksheet is empty, add headers
                    if (startRow == 1)
                    {
                        worksheet.Cells[1, 1].Value = "Product Name";
                        worksheet.Cells[1, 2].Value = "Quantity";
                        worksheet.Cells[1, 3].Value = "Unit Price";
                        worksheet.Cells[1, 4].Value = "Total Price";
                        worksheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;
                        startRow = 2; // Move to the next row for data
                    }

                    // Check if there are any receipt items


                    // Add each receipt item to the worksheet
                    foreach (var item in ReceiptItems)
                    {
                        worksheet.Cells[startRow, 1].Value = item.Name;
                        worksheet.Cells[startRow, 2].Value = item.Quantity;
                        worksheet.Cells[startRow, 3].Value = item.UnitPrice;
                        worksheet.Cells[startRow, 4].Value = item.TotalPrice;
                        startRow++;
                    }

                    // Optional: Add a formula for the total
                    worksheet.Cells[startRow, 3].Value = "Total";
                    worksheet.Cells[startRow, 4].Formula = $"SUM(D2:D{startRow - 1})";
                    worksheet.Cells[startRow, 3, startRow, 4].Style.Font.Bold = true;

                    // Auto-fit columns for better readability
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Save the changes to the file
                    await package.SaveAsync();
                }

                // Optionally clear the ReceiptItems after saving
                //ReceiptItems.Clear(); // Uncomment this line if you want to clear items after saving
                await DisplayAlert("Success", "Receipt saved successfully!", "OK");

        }

        // Event handler for Save button (change this to your existing save button's Clicked event)
        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            await SaveReceiptItemsToSelectedFileAsync();
        }

        private void OnFileCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Find the ExcelFile object associated with this checkbox
            var checkbox = (CheckBox)sender;
            var selectedFile = checkbox.BindingContext as ExcelFile;

            if (selectedFile != null)
            {
                // Deselect all other files if one is selected
                foreach (var file in ExcelFiles)
                {
                    file.IsSelected = file == selectedFile && e.Value;
                }
            }
        }
    }
}
