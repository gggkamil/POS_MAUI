using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButchersCashier.Services
{
    public class ReceiptSaveService : IReceiptSaveService
    {
        public event Func<Task> OnSaveRequested;

        public async Task RequestSaveAsync()
        {
            if (OnSaveRequested != null)
            {
                await OnSaveRequested.Invoke();
            }
        }
    }
}
