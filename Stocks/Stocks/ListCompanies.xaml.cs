using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Stocks
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListCompanies : ContentPage
    {
        private List<SelectableData<Company>> DataList { get; set; }

        public ListCompanies()
        {
            InitializeComponent();

            DataList = new List<SelectableData<Company>>();

            Company c1 = new Company();
            c1.Name = "Apple";
            c1.Symbol = "AAPL";
            c1.ImageSource = "apple_logo.png";

            Company c2 = new Company();
            c2.Name = "Intel";
            c2.Symbol = "INTL";
            c2.ImageSource = "intel_logo.png";

            SelectableData<Company> s1 = new SelectableData<Company>();
            s1.Data = c1;
            s1.Selected = false;

            SelectableData<Company> s2 = new SelectableData<Company>();
            s2.Data = c2;
            s2.Selected = false;

            DataList.Add(s1);
            DataList.Add(s2);

            MyListView.ItemsSource = DataList;
        }

    }
}
