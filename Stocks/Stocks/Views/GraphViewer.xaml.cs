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
        public enum ExtensionType { Week = 0, Month = 1};

        private const string API_KEY = "560d207f7ecf0ee3b6a05584942b6a73";
        private const string URL = "https://marketdata.websol.barchart.com/getHistory.json?apikey={0}&symbol={1}&type=daily&startDate={2}";
        private List<Company> Companies;
        private ExtensionType Type;

        public GraphViewer(List<Company> Companies, ExtensionType Type)
        {
            InitializeComponent();

            this.Companies = Companies;
            this.Type = Type;

            SelectPickerIndex();

            RequestAsync();
        }

        private void SelectPickerIndex()
        {
            Picker picker = (Picker)this.FindByName("Picker");

            if (Type == ExtensionType.Week)
            {
                picker.SelectedIndex = 0;
            }
            else
            {
                picker.SelectedIndex = 1;
            }
        }

        private async void RequestAsync()
        {
            string RequestDate = GetRequestDate();

            for (int i = 0; i < Companies.Count; i++)
            {
                using (HttpClient client = new HttpClient())
                    try
                    {
                        HttpResponseMessage message = await client.GetAsync(string.Format(URL, API_KEY, Companies[i].Symbol, RequestDate));

                        System.Diagnostics.Debug.WriteLine("statusCode: " + message.StatusCode);
                        if (message.StatusCode == HttpStatusCode.OK)
                        {
                            var response = JsonConvert.DeserializeObject(await message.Content.ReadAsStringAsync());
                            System.Diagnostics.Debug.WriteLine(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("An error occurred", ex.Message, "OK");
                    }
            }
        }

        private string GetRequestDate()
        {
            DateTime Date = DateTime.Now;
            if (Type == ExtensionType.Week)
            {
                Date = DateTime.Now.AddDays(-7);
            } else if (Type == ExtensionType.Month)
            {
                Date = DateTime.Now.AddDays(-30);
            }

            string RequestDate = Date.ToString("yyyyMMdd");
            return RequestDate;
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != ((int)Type))
            {
                ExtensionType RangeType = (ExtensionType)selectedIndex;

                new GraphViewer(Companies, RangeType);
            }
        }
    }
}