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

        private const string API_KEY = "1924509c26e52c0ca4466a3da9613d1e";
        private const string URL = "https://marketdata.websol.barchart.com/getHistory.json?apikey={0}&symbol={1}&type=daily&startDate={2}";

        private List<Company> Companies;
        private ExtensionType Type;
        private List<JArray> Data;
        private bool DataLoaded = false;

        private int MaxValueY;
        private int MinValueY;

        private int RangeLimits;
        private int IncreaseUnit;

        private int CanvasWidth;
        private int CanvasHeight;

        private const double CANVAS_WIDTH = 0.2;
        private const double CANVAS_HEIGHT = 0.2;

        private const float HORIZONTAL_PADDING = 0.125f;
        private const float VERTICAL_PADDING = 0.125f;

        private const float AXIS_Y_HORIZONTAL_PADDING = 0.07f;
        private const float AXIS_X_VERTICAL_PADDING = 0.04f;

        public GraphViewer(List<Company> Companies, ExtensionType Type)
        {
            InitializeComponent();

            this.Companies = Companies;
            this.Type = Type;

            ShowLoadingSymbol();
            SelectPickerIndex();

            string RequestDate = GetRequestDate();
            FetchData(Companies, RequestDate);
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

        void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != ((int)Type) && selectedIndex != -1)
            {
                ExtensionType RangeType = (ExtensionType)selectedIndex;
                System.Diagnostics.Debug.WriteLine(selectedIndex);

                var vUpdatedPage = new GraphViewer(Companies, RangeType);
                Navigation.InsertPageBefore(vUpdatedPage, this);
                Navigation.PopAsync();
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

        void OnDrawGraph(object sender, SKPaintSurfaceEventArgs args)
        {
            if (DataLoaded)
            {
                // get the canvas
                SKCanvas canvas = args.Surface.Canvas;

                // clear the canvas with a transparent color
                canvas.Clear();

                // draw the graph
                DrawGraph(Data, canvas, args.Info.Width, args.Info.Height);
            }
        }

        private async void FetchData(List<Company> companies, string startDate)
        {
            List<JArray> data = new List<JArray>();
            
            foreach (Company company in companies)
            {
                // fetch data
                JArray compData = await RequestAsync(company.Symbol, startDate);
                if (compData == null) return;

                data.Add(compData);
            }
            Data = data;
            DataLoaded = true;
            HideLoadingSymbol();

            GraphCanvasView.InvalidateSurface();

            /*
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
            */
        }

        private List<JArray> GetLimits(List<JArray> dataArray)
        {
            List<float> maxQuotes = new List<float>();
            List<float> minQuotes = new List<float>();
            List<JArray> array = dataArray;

            foreach (JArray data in dataArray)
            {
                // list of all close session quotes
                List<float> quotes = data.Select(elem => (float)elem["close"]).ToList();
                maxQuotes.Add(quotes.Max());
                minQuotes.Add(quotes.Min());
            }
            if (dataArray.Count > 1)
            {
                if (maxQuotes[0] < maxQuotes[1])
                {
                    array.Reverse();
                }
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
                // number of integer digits in range
                int noDigits = (int)Math.Floor(Math.Log10(range) + 1);

                // unit to increase in graph
                unit = (int)Math.Pow(10, noDigits - 1);

                if (2 * unit * noDigits > maxY) unit = (int)Math.Pow(10, noDigits - 2);


                // max Y to show in graph
                if (maxQuote % unit != 0)
                    maxY = (int)(maxQuote + (unit - maxQuote % unit));

                // min Y to show in graph
                minY = (int)(minQuote - minQuote % unit);
            }

            MaxValueY = maxY;
            MinValueY = minY;
            IncreaseUnit = unit;

            RangeLimits = MaxValueY - MinValueY;

            return array;
        }

        private void DrawXAxis(SKCanvas canvas, SKPaint axisPaint, int width, int height)
        {
            float initial_x = HORIZONTAL_PADDING * width;
            float final_x = HORIZONTAL_PADDING * width + CanvasWidth;
            float y = CanvasHeight + VERTICAL_PADDING * height;

            // draw axis x line
            canvas.DrawLine(initial_x, y, final_x, y, axisPaint);
        }

        private void DrawYAxis(SKCanvas canvas, SKPaint axisPaint, SKPaint labelPaint, int width, int height)
        {
            SKPath axisY = new SKPath();
            float axisYCanvas = CanvasHeight + VERTICAL_PADDING * height;
            int axisYValue = MinValueY;

            SKPaint linePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 3
            };

            axisY.MoveTo(HORIZONTAL_PADDING * width, axisYCanvas);

            // show initial axis y value
            canvas.DrawText(axisYValue.ToString(), AXIS_Y_HORIZONTAL_PADDING * width, axisYCanvas, labelPaint);

            canvas.DrawLine(HORIZONTAL_PADDING * width - 5f, axisYCanvas, HORIZONTAL_PADDING * width + 5f, axisYCanvas, linePaint);

            for (int i = 0; i < RangeLimits / IncreaseUnit; i++)
            {
                // increase axis y value
                axisYValue += IncreaseUnit;

                // normalize
                axisYCanvas = 1f - ((float)(axisYValue - MinValueY) / RangeLimits);

                // actual pixel value on canvas
                axisYCanvas = CanvasHeight * axisYCanvas + VERTICAL_PADDING * height;

                // show axis y value
                canvas.DrawText(axisYValue.ToString(), AXIS_Y_HORIZONTAL_PADDING * width, axisYCanvas, labelPaint);

                // show line of the axis on the graph
                canvas.DrawLine(HORIZONTAL_PADDING * width-5f, axisYCanvas, HORIZONTAL_PADDING * width + 5f, axisYCanvas, linePaint);

                // create axis y line
                axisY.LineTo(HORIZONTAL_PADDING * width, axisYCanvas);
            }
            axisY.Close();

            canvas.DrawPath(axisY, axisPaint);
        }

        private void DrawLabelXAxis(int index, SKCanvas canvas, SKPaint labelPaint, JObject session, float x, int width, int height)
        {
            int noLabels = 3;

            if (Type == ExtensionType.Week || index % noLabels == 0)
            {
                // get trading day from json object
                string tradingDay = (string)session["tradingDay"];
                DateTime date = DateTime.ParseExact(tradingDay, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

                // convert format of day to dd/MMM
                String day = date.ToString("dd/MMM");

                float labelX = x;
                float labelY = CanvasHeight + VERTICAL_PADDING * height + (AXIS_X_VERTICAL_PADDING * CanvasHeight);

                // show date
                canvas.DrawText(day, labelX, labelY, labelPaint);
            }
        }

        private void DrawGraph(List<JArray> dataArray, SKCanvas canvas, int width, int height)
        {
            dataArray = GetLimits(dataArray);

            CanvasWidth = width - (int)(CANVAS_WIDTH * width);
            CanvasHeight = height - (int)(CANVAS_HEIGHT * height);

            // label for Axises
            SKPaint labelPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextAlign = SKTextAlign.Center
            };

            // different text sizes depending on platforms
            if (Device.RuntimePlatform == Device.Android)
                labelPaint.TextSize = 25;
            else if (Device.RuntimePlatform == Device.UWP)
                labelPaint.TextSize = 13;

            bool first = true;
            foreach (JArray data in dataArray)
            {
                // quantity to add each iteration
                float xdelta = (float) CanvasWidth / (data.Count - 1);

                // starting point
                float x = HORIZONTAL_PADDING * width;

                // starting point
                float initial_y = CanvasHeight + VERTICAL_PADDING * height;

                // create the graph's path
                SKPath path = new SKPath();

                // create graph outline
                SKPath outline = new SKPath();

                // create contour for graph
                path.MoveTo(x, initial_y);

                for (int i = 0; i < data.Count; i++)
                {
                    // quote information
                    JObject session = (JObject) data[i];

                    // normalize
                    float y = 1 - ((float)session["close"] - MinValueY) / RangeLimits;

                    // actual pixel value
                    y = CanvasHeight * y + VERTICAL_PADDING * height;

                    // if first iteration, create countour, otherwise create outlining line
                    if (i == 0) { outline.MoveTo(x, y); }
                    else outline.LineTo(x, y);
                    
                    path.LineTo(x, y);

                    // draw label for X axis
                    DrawLabelXAxis(i, canvas, labelPaint, session, x, width, height);

                    x += xdelta;
                }
                float final_x = width - ((float)CANVAS_WIDTH - HORIZONTAL_PADDING) * width;
                path.LineTo(final_x, CanvasHeight + VERTICAL_PADDING * height);
                path.Close();

                SKPaint axisPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Black,
                    StrokeWidth = 1
                };

                // draw X Axis
                DrawXAxis(canvas, axisPaint, width, height);

                // draw Y Axis
                DrawYAxis(canvas, axisPaint, labelPaint, width, height);

                // reddish color
                Color firstColor = Color.FromRgb(225, 107, 90);

                // blueish color
                Color secondColor = Color.FromRgb(74, 184, 161);

                SKColor redColor = firstColor.ToSKColor();
                SKColor blueColor = secondColor.ToSKColor();

                // create the stroke paint
                SKPaint strokePaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = first ? blueColor : redColor,
                    StrokeWidth = 4
                };

                // create the fill paint
                SKPaint fillPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = first ? blueColor.WithAlpha(150) : redColor.WithAlpha(150)
                };

                DrawLabels(canvas, data, redColor, blueColor, first, width, height);
                first = false;

                // draw the graph paths
                canvas.DrawPath(outline, strokePaint);
                canvas.DrawPath(path, fillPaint);
            }
        }

        private void DrawLabels(SKCanvas canvas, JArray data, SKColor redColor, SKColor blueColor, bool first, int width, int height)
        {
            // create the stroke paint
            SKPaint labelStroke = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = first ? blueColor : redColor,
                StrokeWidth = 10
            };

            SKPaint titlePaint = new SKPaint
            {
                Color = SKColors.Black
            };

            // different text sizes depending on platforms
            if (Device.RuntimePlatform == Device.Android)
                titlePaint.TextSize = 30;
            else if (Device.RuntimePlatform == Device.UWP)
                titlePaint.TextSize = 15;

            float initial_x = HORIZONTAL_PADDING * width;
            float final_x = HORIZONTAL_PADDING * width + 60f;
            float y = 0f;

            if (first) { y = (VERTICAL_PADDING * height) * 0.3f; }
            else y = (VERTICAL_PADDING * height) * 0.6f;

            // draw first company line
            canvas.DrawLine(initial_x, y, final_x, y, labelStroke);

            JObject session = (JObject)data[0];
            // show initial axis y value
            canvas.DrawText((string)session["symbol"], final_x + 5f, y, titlePaint);

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