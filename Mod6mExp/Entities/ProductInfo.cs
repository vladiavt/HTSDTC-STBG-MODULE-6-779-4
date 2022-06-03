using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.Entities
{
    public class ProductInfo
    {
        public string Product { get; set; }
        public string L1 { get; set; }
        public string L2 { get; set; }
        public string L3 { get; set; }
        public string L4 { get; set; }
        public string MarkFileName { get; set; }
        public int ProductLength { get; set; }
        public bool CheckNut { get; set; }
        public int ProductType { get; set; }
        public ProductInfo()
        {
        }
    }
}
