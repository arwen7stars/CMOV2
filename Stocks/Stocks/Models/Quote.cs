using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Stocks
{
    class Quote : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string ImageSource { get; set; }

        public float _QuotePrice;
        public float QuotePrice
        {
            get
            {
                return _QuotePrice;
            }
            set
            {
                _QuotePrice = value;
                OnPropertyChanged(nameof(QuotePrice));
            }
        }

        public Quote(Company company, float price = 0f)
        {
            Name = company.Name;
            Symbol = company.Symbol;
            ImageSource = company.ImageSource;
            QuotePrice = price;
        }

        public Quote(string symbol)
        {
            Symbol = symbol;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler == null)
                return;

            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
