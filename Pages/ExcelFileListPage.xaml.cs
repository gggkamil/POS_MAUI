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
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault() ?? package.Workbook.Worksheets.Add("Receipts");

 
                    int startRow = worksheet.Dimension?.Rows + 1 ?? 1;

   
                    if (startRow == 1)
                    {
                        worksheet.Cells[1, 1].Value = "Produkt";
                        worksheet.Cells[1, 2].Value = "Ilość";
                        worksheet.Cells[1, 3].Value = "Cena jednostkowa";
                        worksheet.Cells[1, 4].Value = "Cena końcowa";
                        worksheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;
                        startRow = 2; 
                    }

                  


       
                    foreach (var item in ReceiptItems)
                    {
                        worksheet.Cells[startRow, 1].Value = item.Name;
                        worksheet.Cells[startRow, 2].Value = item.Quantity;
                        worksheet.Cells[startRow, 3].Value = item.UnitPrice;
                        worksheet.Cells[startRow, 4].Value = item.TotalPrice;
                        startRow++;
                    }

             
                    worksheet.Cells[startRow, 3].Value = "Suma";
                    worksheet.Cells[startRow, 4].Formula = $"SUM(D2:D{startRow - 1})";
                    worksheet.Cells[startRow, 3, startRow, 4].Style.Font.Bold = true;

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
