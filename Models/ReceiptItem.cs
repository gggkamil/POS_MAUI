using System.ComponentModel;

public class ReceiptItem : INotifyPropertyChanged
{
    public string Name { get; set; }

    private decimal _quantity;
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(TotalPrice)); // Update total when quantity changes
        }
    }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice => Quantity * UnitPrice;

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}