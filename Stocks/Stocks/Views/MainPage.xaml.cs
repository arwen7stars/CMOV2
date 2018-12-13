using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Stocks
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        // Button Event to Navigate to Current Quotes Page
        private async void Current_Quotes(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CurrentQuotesPage());
        }

        // Button Event to Navigate to Evolution Graph Page
        private async void Evolution_Graph(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ListCompaniesPage());
        }
    }
}
