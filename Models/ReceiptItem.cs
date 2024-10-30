using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using CashierApp;

public class ReceiptItem : INotifyPropertyChanged
{
    public string Name { get; set; }
    private decimal _quantity;
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }

    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    public ICommand DeleteCommand { get; }

    private readonly ProductsPage _productsPage;

    // Constructor now requires a ProductsPage instance
    public ReceiptItem(ProductsPage productsPage)
    {
        _productsPage = productsPage;
        DeleteCommand = new Command(() => _productsPage.DeleteReceiptItem(this)); // Initialize the delete command
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public void SetQuantityFromText(string text)
    {
        if (decimal.TryParse(text.Replace(".", ","), NumberStyles.Any, CultureInfo.CurrentCulture, out decimal parsedValue))
        {
            Quantity = parsedValue;
        }
    }
}