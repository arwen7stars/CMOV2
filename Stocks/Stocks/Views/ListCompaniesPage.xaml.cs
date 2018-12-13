using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Stocks
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListCompaniesPage : ContentPage
    {
        private List<SelectableData<Company>> CompanyList { get; set; }

        public ListCompaniesPage()
        {
            InitializeComponent();

            CompanyList = FillCompaniesList();
            Companies.ItemsSource = CompanyList;
        }


        private List<SelectableData<Company>> FillCompaniesList() {
            CompanyList = new List<SelectableData<Company>>();
            List<Company> companies = ListCompanies.Companies;

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
                await Navigation.PushAsync(new GraphViewer(selectedCompanies, GraphViewer.ExtensionType.Week));
            }
        }
    }
}
