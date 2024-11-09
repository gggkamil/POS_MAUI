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

        // New property for storing the customer name
        public string CustomerName { get; set; } = "Default Customer";

        private readonly IReceiptSaveService _receiptSaveService;

        public ExcelFileListPage(ObservableCollection<ReceiptItem> receiptItems, IReceiptSaveService receiptSaveService)
        {
            InitializeComponent();
            BindingContext = this;
            ReceiptItems = receiptItems;
            LoadExcelFiles();
            _receiptSaveService = receiptSaveService;

            // Update to use the parameterless wrapper
            _receiptSaveService.OnSaveRequested += SaveReceiptItemsToSelectedFileAsync;

            MessagingCenter.Subscribe<ProductsPage, string>(this, "SaveReceiptItems", async (sender, customerName) =>
            {
                await SaveReceiptItemsToSelectedFileAsync(customerName);
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

        // Parameterless wrapper to satisfy the delegate
        public async Task SaveReceiptItemsToSelectedFileAsync()
        {
            await SaveReceiptItemsToSelectedFileAsync(CustomerName);
        }

        // Main method to save receipt items with a specific customer name
        public async Task SaveReceiptItemsToSelectedFileAsync(string customerName)
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
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name == "Customer Receipts")
                                ?? package.Workbook.Worksheets.Add("Customer Receipts");

                int startRow = worksheet.Dimension?.Rows + 1 ?? 1;

                if (startRow == 1)
                {
                    worksheet.Cells[1, 1].Value = "Imię i nazwisko";
                    worksheet.Cells[1, 2].Value = "Suma";
                    worksheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;
                    startRow = 2;
                }

                decimal totalSum = ReceiptItems.Sum(item => item.TotalPrice);

                worksheet.Cells[startRow, 1].Value = customerName;
                worksheet.Cells[startRow, 2].Value = totalSum.ToString("C");

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

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
