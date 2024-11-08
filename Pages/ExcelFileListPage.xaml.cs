using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using ButchersCashier.Models;
using Microsoft.Maui.Controls;
using OfficeOpenXml; // Ensure you have the EPPlus library for Excel manipulation
using System.Threading.Tasks;
using ButchersCashier.Services;
using CashierApp;

namespace CashierApps
{
    public partial class ExcelFileListPage : ContentPage
    {
        public ObservableCollection<ExcelFile> ExcelFiles { get; set; } = new ObservableCollection<ExcelFile>();

    
        public ObservableCollection<ReceiptItem> ReceiptItems { get; private set; }
        private readonly IReceiptSaveService _receiptSaveService;
        public ExcelFileListPage(ObservableCollection<ReceiptItem> receiptItems, IReceiptSaveService receiptSaveService)
        {
            InitializeComponent();
            BindingContext = this;
            ReceiptItems = receiptItems; 
            LoadExcelFiles(); 
            _receiptSaveService = receiptSaveService;
            _receiptSaveService.OnSaveRequested += SaveReceiptItemsToSelectedFileAsync;
            MessagingCenter.Subscribe<ProductsPage>(this, "SaveReceiptItems", async (sender) =>
            {
                await SaveReceiptItemsToSelectedFileAsync();
            });
        }

        private void LoadExcelFiles()
        {
            var folderPath = @"C:\Kasa\WZ";
            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath, "*.xlsx");
                foreach (var file in files)
                {
                    ExcelFiles.Add(new ExcelFile
                    {
                        FileName = Path.GetFileName(file),
                        FilePath = file,
                        IsSelected = false 
                    });
                }
            }
        }

        public string GetSelectedFilePath()
        {
   
            var selectedFile = ExcelFiles.FirstOrDefault(f => f.IsSelected);
            return selectedFile?.FilePath; 
        }


        public async Task SaveReceiptItemsToSelectedFileAsync()
        {
            var selectedFilePath = GetSelectedFilePath();

            if (string.IsNullOrEmpty(selectedFilePath))
            {
                return;
            }
            if (ReceiptItems.Count == 0)
            {
                await DisplayAlert("WZ", "Brak produktów do zapisania", "OK");
                return;
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(selectedFilePath)))
            {
                // Ensure there's a "Receipts" tab to store individual receipt items
                var receiptWorksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name == "Rachunki")
                                       ?? package.Workbook.Worksheets.Add("Rachunki");

                // Start row for appending receipt items
                int startRow = receiptWorksheet.Dimension?.Rows + 1 ?? 1;

                // Add header row if the worksheet is empty
                if (startRow == 1)
                {
                    receiptWorksheet.Cells[1, 1].Value = "Produkt";
                    receiptWorksheet.Cells[1, 2].Value = "Ilość";
                    receiptWorksheet.Cells[1, 3].Value = "Cena jednostkowa";
                    receiptWorksheet.Cells[1, 4].Value = "Cena końcowa";
                    receiptWorksheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;
                    startRow = 2;
                }

                // Add current receipt items to "Receipts" tab
                foreach (var item in ReceiptItems)
                {
                    receiptWorksheet.Cells[startRow, 1].Value = item.Name;
                    receiptWorksheet.Cells[startRow, 2].Value = item.Quantity;
                    receiptWorksheet.Cells[startRow, 3].Value = item.UnitPrice;
                    receiptWorksheet.Cells[startRow, 4].Value = item.TotalPrice;
                    startRow++;
                }

                // Add a "Total" row
                receiptWorksheet.Cells[startRow, 3].Value = "Suma";
                receiptWorksheet.Cells[startRow, 4].Formula = $"SUM(D2:D{startRow - 1})";
                receiptWorksheet.Cells[startRow, 3, startRow, 4].Style.Font.Bold = true;

                // Adjust column widths
                receiptWorksheet.Cells[receiptWorksheet.Dimension.Address].AutoFitColumns();

                // Ensure there's a "Total Quantity" tab
                var totalQuantityWorksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name == "Suma wydań")
                                             ?? package.Workbook.Worksheets.Add("Suma wydań");

                var productTotals = new Dictionary<string, double>();

               
                int totalRow = 2;
                if (totalQuantityWorksheet.Dimension != null)
                {
                    for (int row = 2; row <= totalQuantityWorksheet.Dimension.Rows; row++)
                    {
                        string productName = totalQuantityWorksheet.Cells[row, 1].Text;
                        double quantity = totalQuantityWorksheet.Cells[row, 2].GetValue<double>();
                        productTotals[productName] = quantity;
                    }
                }

                foreach (var item in ReceiptItems)
                {
                    if (productTotals.ContainsKey(item.Name))
                    {
                        productTotals[item.Name] += (double)item.Quantity;  
                    }
                    else
                    {
                        productTotals[item.Name] = (double)item.Quantity; 
                    }
                }

                // Clear the "Total Quantity" tab and add header if necessary
                totalQuantityWorksheet.Cells.Clear();
                totalQuantityWorksheet.Cells[1, 1].Value = "Produkt";
                totalQuantityWorksheet.Cells[1, 2].Value = "Łączna Ilość";
                totalQuantityWorksheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;

                // Populate the "Total Quantity" tab with updated totals
                totalRow = 2;
                foreach (var kvp in productTotals)
                {
                    totalQuantityWorksheet.Cells[totalRow, 1].Value = kvp.Key;
                    totalQuantityWorksheet.Cells[totalRow, 2].Value = kvp.Value;
                    totalRow++;
                }

                // Adjust column widths in the "Total Quantity" tab
                totalQuantityWorksheet.Cells[totalQuantityWorksheet.Dimension.Address].AutoFitColumns();

                // Save the package
                await package.SaveAsync();
            }

            await DisplayAlert("WZ", $"Rachunek dodano do WZ! {selectedFilePath}", "OK");
        }



        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            await SaveReceiptItemsToSelectedFileAsync();
        }

        private void OnFileCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var selectedFile = (sender as CheckBox)?.BindingContext as ExcelFile;

            if (selectedFile == null || !selectedFile.IsSelected) return;
       
      
                foreach (var file in ExcelFiles)
                {
                    if (file != selectedFile && file.IsSelected)
                    {
                        file.IsSelected = false;
                    }
                }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _receiptSaveService.OnSaveRequested -= SaveReceiptItemsToSelectedFileAsync;
        }
    }
}
