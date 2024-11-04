using Android.Graphics;
using Android.Graphics.Pdf;
using Android.OS;
using Android.Print;
using Android.Print.Pdf;
using System.IO;
using System.Text;

public class ReceiptPrintDocumentAdapter : PrintDocumentAdapter
{
    private readonly string _receiptText;
    private PrintAttributes _printAttributes;

    public ReceiptPrintDocumentAdapter(string receiptText)
    {
        _receiptText = receiptText;
    }

    public override void OnLayout(PrintAttributes oldAttributes, PrintAttributes newAttributes,
                                  CancellationSignal cancellationSignal,
                                  LayoutResultCallback callback, Bundle extras)
    {
        _printAttributes = newAttributes; 
        var pdi = new PrintDocumentInfo.Builder("receipt.pdf")
                    .SetContentType(PrintContentType.Document)
                    .Build();
        callback.OnLayoutFinished(pdi, true);
    }

    public override void OnWrite(PageRange[] pages, ParcelFileDescriptor destination,
                                 CancellationSignal cancellationSignal,
                                 WriteResultCallback callback)
    {
        using var pdfDocument = new PrintedPdfDocument(Android.App.Application.Context, _printAttributes);
        var pageInfo = new PdfDocument.PageInfo.Builder(300, 600, 1).Create();
        var page = pdfDocument.StartPage(pageInfo);

        if (cancellationSignal.IsCanceled)
        {
            callback.OnWriteCancelled();
            pdfDocument.Close();
            return;
        }

        var paint = new Android.Graphics.Paint
        {
            Color = Android.Graphics.Color.Black,
            TextSize = 12,
            AntiAlias = true
        };

        var lines = _receiptText.Split('\n');
        float yPosition = 20;
        foreach (var line in lines)
        {
            page.Canvas.DrawText(line, 10, yPosition, paint);
            yPosition += 20;
        }

        pdfDocument.FinishPage(page);

        try
        {
            using (var outputStream = new System.IO.FileStream(destination.FileDescriptor.ToString(), FileMode.Create))
            {
                pdfDocument.WriteTo(outputStream);
            }
        }
        catch (Exception ex)
        {
   
            callback.OnWriteFailed(ex.Message);
            return;
        }

        callback.OnWriteFinished(new[] { PageRange.AllPages });
        pdfDocument.Close();
    }
}
