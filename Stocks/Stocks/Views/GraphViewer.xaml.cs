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
        private const string API_KEY = "560d207f7ecf0ee3b6a05584942b6a73";
        private const string URL = "https://marketdata.websol.barchart.com/getHistory.json?apikey={0}&symbol={1}&type=daily&startDate={2}";


        public GraphViewer(List<Company> companies, string startDate)
        {
            InitializeComponent();
            Title = "Quote Graph";
            DrawCanvas(companies, startDate);
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

            // set page content
            Content = canvasView;
        }


        private void DrawGraph(List<JArray> dataArray, SKCanvas canvas, int width, int height)
        {
            List<float> maxQuotes = new List<float>();
            List<float> minQuotes = new List<float>();
            foreach (JArray data in dataArray)
            {
                // list of all close session quotes
                List<float> quotes = data.Select(elem => (float)elem["close"]).ToList();
                maxQuotes.Add(quotes.Max());
                minQuotes.Add(quotes.Min());
            }

            // get the max of all max, and the min of all min
            float maxQuote = maxQuotes.Max();
            float minQuote = minQuotes.Min();

            bool first = true;
            foreach (JArray data in dataArray)
            {
                float xdelta = (float)width / (data.Count - 1);
                float x = 0;

                // create the path
                SKPath path = new SKPath();
                path.MoveTo(0, height);

                foreach (JObject session in data)
                {
                    // normalize
                    float y = 1 - ((float)session["close"] - minQuote) / (maxQuote - minQuote);

                    // actual pixel value
                    y = height * y;

                    path.LineTo(x, y);

                    Console.WriteLine(session["tradingDay"] + ": (" + x + ", " + y + ")");

                    x += xdelta;
                }
                path.LineTo(width, height);
                path.Close();

                // create the paint
                SKPaint strokePaint = new SKPaint
                {
                    Style = SKPaintStyle.StrokeAndFill,
                    Color = first ? SKColors.Blue.WithAlpha(50) : SKColors.Red.WithAlpha(50),
                    StrokeWidth = 1
                };
                first = false;

                // draw the path
                canvas.DrawPath(path, strokePaint);
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