using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Stocks
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GraphViewer : ContentPage
    {
        private const string API_KEY = "560d207f7ecf0ee3b6a05584942b6a73";
        private const string URL = "https://marketdata.websol.barchart.com/getHistory.json?apikey={0}&symbol={1}&type=daily&startDate={2}";


        public GraphViewer(List<Company> companies)
        {
            InitializeComponent();
            Console.WriteLine("ola: " + companies.Count);
            RequestAsync();
        }

        private async void RequestAsync()
        {
            using (HttpClient client = new HttpClient())
                try
                {
                    HttpResponseMessage message = await client.GetAsync(string.Format(URL, API_KEY, "IBM", "20181106"));

                    Console.WriteLine("statusCode: " + message.StatusCode);
                    if (message.StatusCode == HttpStatusCode.OK)
                    {
                        var response = JsonConvert.DeserializeObject(await message.Content.ReadAsStringAsync());
                        Console.WriteLine(response);

                    }   
                }
                catch (Exception ex)
                {
                    await DisplayAlert("An error occurred", ex.Message, "OK");
                }
        }
    }
}