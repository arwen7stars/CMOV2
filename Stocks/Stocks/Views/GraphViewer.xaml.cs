using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;

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
            string RequestDate = GetRequestDate();

            DrawCanvas(Companies, RequestDate);
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

        private async void DrawCanvas(List<Company> companies, string startDate)
        {
            List<JArray> data = new List<JArray>();
            
            foreach (Company company in companies)
            {
                // fetch data
                JArray compData = await RequestAsync(company.Symbol, startDate);
                if (compData == null) return;

                data.Add(compData);
            }      

            // create the canvas
            SKCanvasView canvasView = new SKCanvasView();

            canvasView.PaintSurface += (object sender, SKPaintSurfaceEventArgs args) =>
            {
                // get the canvas
                SKCanvas canvas = args.Surface.Canvas;

                // clear the canvas with a transparent color
                canvas.Clear();

                // draw the graph
                DrawGraph(data, canvas, args.Info.Width, args.Info.Height);
            };

            Content = canvasView;
        }


        private void DrawGraph(List<JArray> dataArray, SKCanvas canvas, int width, int height)
        {
            List<float> maxQuotes = new List<float>();
            List<float> minQuotes = new List<float>();
            int canvasWidth = width - (int)(0.1 * width);
            int canvasHeight = height - (int)(0.1 * height);

            foreach (JArray data in dataArray)
            {
                // list of all close session quotes
                List<float> quotes = data.Select(elem => (float)elem["close"]).ToList();
                maxQuotes.Add(quotes.Max());
                minQuotes.Add(quotes.Min());
            }

            // get the max of all maxList<float> minQuotes = new List<float>();
            float maxQuote = maxQuotes.Max();
            float minQuote = minQuotes.Min();

            SKPaint labelPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 13
            };

            bool first = true;
            foreach (JArray data in dataArray)
            {
                float xdelta = (float)canvasWidth / (data.Count - 1);
                float x = 0.05f * width;

                // create the path
                SKPath path = new SKPath();
                SKPath outline = new SKPath();
                path.MoveTo(x, canvasHeight);

                for(int i = 0; i < data.Count; i++)
                {
                    JObject session = (JObject) data[i];

                    float range = maxQuote - minQuote;

                    int noDigits = (int)Math.Floor(Math.Log10(range) + 1);
                    int unit = (int)Math.Pow(10, noDigits - 1);

                    float maxY = maxQuote;
                    if (maxQuote % 10 != 0)
                        maxY = maxQuote + (unit - maxQuote % unit);

                    float minY = minQuote - minQuote % unit;

                    // normalize
                    float y = 1 - ((float)session["close"] - minQuote) / (maxQuote - minQuote);

                    // actual pixel value
                    y = canvasHeight * y;

                    if (i == 0)
                    {
                        outline.MoveTo(x, y);
                    } else {
                        outline.LineTo(x, y);
                    }

                    path.LineTo(x, y);

                    string tradingDay = (string)session["tradingDay"];
                    DateTime date = DateTime.ParseExact(tradingDay, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    String day = date.ToString("dd/MMM");

                    float labelX = x - 25f;
                    float labelY = canvasHeight + (0.03f * canvasHeight);

                    canvas.DrawText(day, labelX, labelY, labelPaint);
                    Console.WriteLine(session["tradingDay"] + ": (" + x + ", " + y + ")");

                    x += xdelta;
                }
                path.LineTo(width-0.05f*width, canvasHeight);
                path.Close();

                // create the paint
                SKPaint strokePaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = first ? SKColors.Blue : SKColors.Red,
                    StrokeWidth = 1
                };

                // create the paint
                SKPaint fillPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = first ? SKColors.Blue.WithAlpha(100) : SKColors.Red.WithAlpha(100)
                };
                first = false;

                // draw the path
                canvas.DrawPath(outline, strokePaint);
                canvas.DrawPath(path, fillPaint);
            }
        }


        private async Task<JArray> RequestAsync(string symbol, string startDate)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage message = await client.GetAsync(string.Format(URL, API_KEY, symbol, startDate));
                    if (!message.IsSuccessStatusCode)
                    {
                        throw new Exception("API request failed with code " + message.StatusCode);
                    }

                    JObject response = JObject.Parse(await message.Content.ReadAsStringAsync());

                    int statusCode = (int)response["status"]["code"];
                    if (statusCode != 200)
                    {
                        throw new Exception("API request failed with code " + statusCode);
                    }

                    return (JArray)response["results"];
                }
                catch (Exception ex)
                {
                    await DisplayAlert("An error occurred", ex.Message, "OK");
                }
            }                
            return null;
        }
    }
}