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
        private List<SelectableData<Company>> CompanyList { get; set; }

        public ListCompanies()
        {
            InitializeComponent();

            CompanyList = FillCompaniesList();
            MyListView.ItemsSource = CompanyList;
        }


        private List<SelectableData<Company>> FillCompaniesList() {
            CompanyList = new List<SelectableData<Company>>();
            List<Company> companies = new List<Company>
            {
                new Company("AMD", "AMD", "amd_logo.png"),
                new Company("Apple", "AAPL", "apple_logo.png"),
                new Company("Facebook", "FB", "facebook_logo.png"),
                new Company("Google", "GOOGL", "google_logo.png"),
                new Company("Hewlett Packard", "HPE", "hp_logo.png"),
                new Company("IBM", "IBM", "ibm_logo.png"),
                new Company("Intel", "INTC", "intel_logo.png"),
                new Company("Microsoft", "MSFT", "microsoft_logo.png"),
                new Company("Oracle", "ORCL", "oracle_logo.gif"),
                new Company("Twitter", "TWTR", "twitter_logo.png")
            };

            for (int i = 0; i < companies.Count; i++)
            {
                CompanyList.Add(new SelectableData<Company>(companies[i]));
            }

            return CompanyList;
        }

        async void Button_Clicked(object sender, EventArgs e)
        {
            int selected = 0;
            List<Company> selectedCompanies = new List<Company>();
            for (int i = 0; i < CompanyList.Count; i++)
            {
                if (CompanyList[i].Selected) {
                    selected++;
                    selectedCompanies.Add(CompanyList[i].Data);
                }
            }

            if (selected == 0)
            {
                await DisplayAlert("Invalid Selection", "No companies selected", "OK");
            }
            else if (selected > 2)
            {
                await DisplayAlert("Invalid Selection", "Can only select up to 2 companies", "OK");
            }
            else
            {
                await Navigation.PushAsync(new GraphViewer(selectedCompanies, "20181113"));
            }

        }
    }
}
