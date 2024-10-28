using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButchersCashier.Models
{
    public class Product
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string ImagePath { get; set; }
        public string QuantityType { get; set; }
    }
}
