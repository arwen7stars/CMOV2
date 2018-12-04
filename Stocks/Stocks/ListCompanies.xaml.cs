﻿using System;
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

            DataList = fillCompaniesList();
            MyListView.ItemsSource = DataList;
        }


        private List<SelectableData<Company>> fillCompaniesList() {
            DataList = new List<SelectableData<Company>>();
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
                DataList.Add(new SelectableData<Company>(companies[i]));
            }

            return DataList;
        }

    }
}
