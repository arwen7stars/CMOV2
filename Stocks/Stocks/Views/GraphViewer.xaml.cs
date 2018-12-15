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

        private const string API_KEY = "7f0ecc205dac93675a6fea8f36e2ec0c";
        private const string URL = "https://marketdata.websol.barchart.com/getHistory.json?apikey={0}&symbol={1}&type=daily&startDate={2}";

        private List<Company> Companies;
        private ExtensionType Type;

        private int MaxValueY;
        private int MinValueY;
        private int IncreaseUnit;

        private const float HORIZONTAL_PADDING = 0.075f;
        private const float VERTICAL_PADDING = 0.075f;

        private const float AXIS_X_HORIZONTAL_PADDING = 0.04f;
        private const float AXIS_X_VERTICAL_PADDING = 25f;
        private const float AXIS_Y_HORIZONTAL_PADDING = 0.025f;
        private const float AXIS_Y_VERTICAL_PADDING = 0f;

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

        private void GetLimits(List<JArray> dataArray)
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

            // get the max of all maxList<float> minQuotes = new List<float>();
            float maxQuote = maxQuotes.Max();
            float minQuote = minQuotes.Min();

            // range for quote values
            float range = maxQuote - minQuote;
            int minY = (int)Math.Floor(minQuote);
            int maxY = (int)Math.Ceiling(maxQuote);
            int unit = 1;

            if (range > 1)
            {
                System.Diagnostics.Debug.WriteLine("Range: " + range);

                // number of integer digits in range
                int noDigits = (int)Math.Floor(Math.Log10(range) + 1);

                System.Diagnostics.Debug.WriteLine("Digits: " + noDigits);

                // unit to increase in graph
                if (noDigits > 2)
                {
                    unit = (int)Math.Pow(10, noDigits - 2);
                } else unit = (int)Math.Pow(10, noDigits - 1);

                // max Y to show in graph
                if (maxQuote % unit != 0)
                    maxY = (int)(maxQuote + (unit - maxQuote % unit));

                // min Y to show in graph
                minY = (int)(minQuote - minQuote % unit);
            }

            MaxValueY = maxY;
            MinValueY = minY;
            IncreaseUnit = unit;

            System.Diagnostics.Debug.WriteLine("Max Quote: " + maxQuote + " max Y: " + maxY);
            System.Diagnostics.Debug.WriteLine("Min Quote: " + minQuote + " min Y: " + minY);
        }

        private void DrawYAxis(SKCanvas canvas, SKPaint labelPaint, int RangeLimits, int canvasHeight, int width, int height)
        {
            SKPath axisY = new SKPath();
            float axisYCanvas = canvasHeight + VERTICAL_PADDING * height;
            int axisYValue = MinValueY;

            SKPaint linePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 3
            };

            axisY.MoveTo(HORIZONTAL_PADDING * width, axisYCanvas);

            canvas.DrawText(axisYValue.ToString(), AXIS_Y_HORIZONTAL_PADDING * width, axisYCanvas + AXIS_Y_VERTICAL_PADDING*height, labelPaint);
            canvas.DrawLine(HORIZONTAL_PADDING * width - 5f, axisYCanvas, HORIZONTAL_PADDING * width + 5f, axisYCanvas, linePaint);

            for (int i = 0; i < RangeLimits / IncreaseUnit; i++)
            {
                // increase axis y value
                axisYValue += IncreaseUnit;

                // normalize
                axisYCanvas = 1f - ((float)(axisYValue - MinValueY) / RangeLimits);

                // actual pixel value on canvas
                axisYCanvas = canvasHeight * axisYCanvas + VERTICAL_PADDING * height;

                // show axis y value
                canvas.DrawText(axisYValue.ToString(), AXIS_Y_HORIZONTAL_PADDING * width, axisYCanvas + AXIS_Y_VERTICAL_PADDING*height, labelPaint);
                canvas.DrawLine(HORIZONTAL_PADDING * width-5f, axisYCanvas, HORIZONTAL_PADDING * width + 5f, axisYCanvas, linePaint);

                // create axis y line
                axisY.LineTo(HORIZONTAL_PADDING * width, axisYCanvas);
            }
            axisY.Close();

            SKPaint axisPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 1
            };

            canvas.DrawPath(axisY, axisPaint);
        }

        private void DrawLabelXAxis(int index, SKCanvas canvas, SKPaint labelPaint, JObject session, float x, int canvasHeight, int width, int height)
        {
            int noLabels = 3;

            if (Type == ExtensionType.Week || index % noLabels == 0)
            {
                string tradingDay = (string)session["tradingDay"];
                DateTime date = DateTime.ParseExact(tradingDay, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                String day = date.ToString("dd/MMM");

                float labelX = x - AXIS_X_VERTICAL_PADDING;
                float labelY = canvasHeight + VERTICAL_PADDING * height + (AXIS_X_HORIZONTAL_PADDING * canvasHeight);

                canvas.DrawText(day, labelX, labelY, labelPaint);
            }
        }


        private void DrawGraph(List<JArray> dataArray, SKCanvas canvas, int width, int height)
        {
            GetLimits(dataArray);
            int RangeLimits = MaxValueY - MinValueY;

            int canvasWidth = width - (int)(0.15 * width);
            int canvasHeight = height - (int)(0.15 * height);

            // label for Axises
            SKPaint labelPaint = new SKPaint
            {
                Color = SKColors.Black
            };

            if (Device.RuntimePlatform == Device.Android)
                labelPaint.TextSize = 25;
            else if (Device.RuntimePlatform == Device.UWP)
                labelPaint.TextSize = 13;

            bool first = true;
            foreach (JArray data in dataArray)
            {
                float xdelta = (float) canvasWidth / (data.Count - 1);
                float x = HORIZONTAL_PADDING * width;

                // create the graph's path
                SKPath path = new SKPath();

                // create graph outline
                SKPath outline = new SKPath();

                // create contour for graph
                path.MoveTo(x, canvasHeight + VERTICAL_PADDING * height);

                for (int i = 0; i < data.Count; i++)
                {
                    // quote information
                    JObject session = (JObject) data[i];

                    // normalize
                    float y = 1 - ((float)session["close"] - MinValueY) / RangeLimits;

                    // actual pixel value
                    y = canvasHeight * y + VERTICAL_PADDING * height;

                    // if first iteration, create countour, otherwise create outlining line
                    if (i == 0) { outline.MoveTo(x, y); }
                    else outline.LineTo(x, y);
                    
                    path.LineTo(x, y);
                    DrawLabelXAxis(i, canvas, labelPaint, session, x, canvasHeight, width, height);

                    x += xdelta;
                }
                path.LineTo(width - HORIZONTAL_PADDING * width, canvasHeight + VERTICAL_PADDING * height);
                path.Close();

                // draw Y Axis
                DrawYAxis(canvas, labelPaint, RangeLimits, canvasHeight, width, height);

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