using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Stocks
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentQuotesPage : ContentPage
    {
        private const string API_KEY = "560d207f7ecf0ee3b6a05584942b6a73";
        private const string URL = "https://marketdata.websol.barchart.com/getQuote.json?apikey={0}&symbols={1}";

        private ObservableCollection<Quote> QuoteList { get; set; }

        public CurrentQuotesPage()
        {
            InitializeComponent();

            ShowLoadingSymbol();
            RequestAsync();
        }

        private async void RequestAsync()
        {
            string Symbols = GetSymbols();

            using (HttpClient client = new HttpClient())
                try
                {
                    HttpResponseMessage message = await client.GetAsync(string.Format(URL, API_KEY, Symbols));

                    Console.WriteLine("statusCode: " + message.StatusCode);
                    if (message.StatusCode == HttpStatusCode.OK)
                    {
                        string result = await message.Content.ReadAsStringAsync();
                        JObject response = JObject.Parse(result);

                        List<Quote> quotesList = new List<Quote>();
                        List<Company> Companies = ListCompanies.Companies;

                        foreach (JObject obj in response["results"].Children<JObject>())
                        {
                            float lastPrice = (float)obj["lastPrice"];
                            string symbol = (string)obj["symbol"];

                            Company Company = Companies.Find(c => c.Symbol == symbol);
                            Quote q = new Quote(Company, lastPrice);
                            quotesList.Add(q);
                        }

                        QuoteList = new ObservableCollection<Quote>(quotesList);

                        Quotes.ItemsSource = QuoteList;
                        HideLoadingSymbol();
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("An error occurred", ex.Message, "OK");
                }

        }

        private void ShowLoadingSymbol()
        {
            IsBusy = true;
            loadingSymbol.IsVisible = true;
            loadingSymbol.IsRunning = true;
        }

        private void HideLoadingSymbol()
        {
            IsBusy = false;
            loadingSymbol.IsVisible = false;
            loadingSymbol.IsRunning = false;
        }

        public string GetSymbols()
        {
            List<Company> companies = ListCompanies.Companies;
            string symbols = "";

            for (int i = 0; i < companies.Count; i++)
            {
                if (i != companies.Count - 1)
                {
                    symbols += companies[i].Symbol + ",";
                }
                else
                {
                    symbols += companies[i].Symbol;
                }
            }

            return symbols;
        }
    }
}