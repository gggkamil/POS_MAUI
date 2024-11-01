using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButchersCashier.Services
{
    public interface IReceiptSaveService
    {
        event Func<Task> OnSaveRequested;
        Task RequestSaveAsync();
    }
}
