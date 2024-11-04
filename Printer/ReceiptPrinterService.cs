using RawPrint;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RawPrint.NetStd;

public class ReceiptPrinterService
{
    private readonly string _printerName;

    public ReceiptPrinterService(string printerName)
    {
        _printerName = printerName;
    }

    public async Task PrintReceiptAsync(string receiptText)
    {
        try
        {
          
            Printer printer = new Printer();

        
            byte[] receiptBytes = Encoding.ASCII.GetBytes(receiptText);

           
            using (var stream = new MemoryStream(receiptBytes))
            {
                // Use the four-parameter PrintRawStream method
                await Task.Run(() => printer.PrintRawStream(
                    _printerName,
                    stream,
                    "Receipt",
                    false      
                ));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Printing failed: {ex.Message}");
        }
    }
}