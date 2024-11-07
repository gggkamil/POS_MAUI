using RawPrint;
using RawPrint.NetStd;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
            byte[] setCharsetCommand = { 0x1B, 0x74, 0x00 }; 

            string asciiText = ReplacePolishCharsWithAscii(receiptText);

            byte[] receiptBytes = Encoding.ASCII.GetBytes(asciiText);

            byte[] allBytes = new byte[setCharsetCommand.Length + receiptBytes.Length];
            Buffer.BlockCopy(setCharsetCommand, 0, allBytes, 0, setCharsetCommand.Length);
            Buffer.BlockCopy(receiptBytes, 0, allBytes, setCharsetCommand.Length, receiptBytes.Length);

            using (var stream = new MemoryStream(allBytes))
            {
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

    private string ReplacePolishCharsWithAscii(string input)
    {
        return input.Replace("ą", "a")
                    .Replace("ć", "c")
                    .Replace("ę", "e")
                    .Replace("ł", "l")
                    .Replace("ń", "n")
                    .Replace("ó", "o")
                    .Replace("ś", "s")
                    .Replace("ź", "z")
                    .Replace("ż", "z")
                    .Replace("Ą", "A")
                    .Replace("Ć", "C")
                    .Replace("Ę", "E")
                    .Replace("Ł", "L")
                    .Replace("Ń", "N")
                    .Replace("Ó", "O")
                    .Replace("Ś", "S")
                    .Replace("Ź", "Z")
                    .Replace("Ż", "Z");

    }
}
