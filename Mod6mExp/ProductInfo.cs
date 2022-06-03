using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine
{
    public class ProductInfo
    {
        private string _product;
        public string Product
        {
            get { return _product; }
            set { _product = value; }
        }

        private string _producttype;
        public string ProductType
        {
            get { return _producttype; }
            set { _producttype = value; }
        }

        private string _l1;
        public string L1
        {
            get { return _l1; }
            set { _l1 = value; }
        }

        private string _l2;
        public string L2
        {
            get { return _l2; }
            set { _l2 = value; }
        }
        private string _l3;
        public string L3
        {
            get { return _l3; }
            set { _l3 = value; }
        }
        private string _l4;
        public string L4
        {
            get { return _l4; }
            set { _l4 = value; }
        }
        private string _markfn;
        public string MarkFN
        {
            get { return _markfn; }
            set { _markfn = value; }
        }
        private double _productlength;
        public double ProductLength
        {
            get { return _productlength; }
            set { _productlength = value; }
        }
        private bool _checknut;
        public bool CheckNut
        {
            get { return _checknut; }
            set { _checknut = value; }
        }
        public ProductInfo()
        {
            Product = "";
            L1 = "";
            L2 = "";
            L3 = "";
            L4 = "";
            MarkFN = "";
            ProductLength = 0;
            CheckNut = false;
            ProductType = "";
        }
    }
}
