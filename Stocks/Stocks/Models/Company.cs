using System;
using System.Collections.Generic;
using System.Text;

namespace Stocks
{
    class Company
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string ImageSource { get; set; }

        public Company(string name, string symbol, string imageSource)
        {
            this.Name = name;
            this.Symbol = symbol;
            this.ImageSource = imageSource;
        }
    }
}
