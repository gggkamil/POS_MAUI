using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButchersCashier.Models
{
    public class SelectableOrder
    {
        public OrderRow Order { get; set; }
        public bool IsSelected { get; set; }
        public string DisplayName => $"ID {Order.OrderId} - {Order.CustomerName}";
    }
}
