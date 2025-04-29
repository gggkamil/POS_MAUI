using System.Collections.ObjectModel;

namespace ButchersCashier.Models
{
    public class OrderRow
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public ObservableCollection<ProductQuantity> ProductQuantities { get; set; }
    }
}
