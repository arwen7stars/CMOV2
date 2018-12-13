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

            CompanyList = fillCompaniesList();
            MyListView.ItemsSource = CompanyList;
        }


        private List<SelectableData<Company>> fillCompaniesList() {
            CompanyList = new List<SelectableData<Company>>();
            List<Company> companies = new List<Company>();

            companies.Add(new Company("AMD", "AMD", "amd_logo.png"));
            companies.Add(new Company("Apple", "AAPL", "apple_logo.png"));
            companies.Add(new Company("Facebook", "FB", "facebook_logo.png"));
            companies.Add(new Company("Google", "GOOGL", "google_logo.png"));
            companies.Add(new Company("Hewlett Packard", "HPE", "hp_logo.png"));
            companies.Add(new Company("IBM", "IBM", "ibm_logo.png"));
            companies.Add(new Company("Intel", "INTC", "intel_logo.png"));
            companies.Add(new Company("Microsoft", "MSFT", "microsoft_logo.png"));
            companies.Add(new Company("Oracle", "ORCL", "oracle_logo.gif"));
            companies.Add(new Company("Twitter", "TWTR", "twitter_logo.png"));

            for(int i = 0; i < companies.Count; i++)
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
                await DisplayAlert("Valid Selection", "Selected " + selected + " companies", "OK");
                await Navigation.PushAsync(new GraphViewer(selectedCompanies, "20181113"));
            }

        }
    }
}
