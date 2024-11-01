using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButchersCashier.Services
{
    public class ReceiptService
    {
        public ObservableCollection<ReceiptItem> ReceiptItems { get; } = new ObservableCollection<ReceiptItem>();
    }

}
