using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BasicBitmapManipulation
{
    /// <summary>
    /// Interaction logic for LoopWindowTest2.xaml
    /// </summary>
    public partial class LoopWindowTest2 : Window
    {
        public DrawingVisual? DVisuals { get; set; }
        public DrawingContext? DContext { get; set; }
        public static Brush ScreenBackground { get; set; } = Brushes.Transparent;
        private RenderTargetBitmap? renderBitmap;

        // FPS tracking
        private DateTime lastFrameTime = DateTime.Now;
        private double currentFps = 0;
        private Queue<double> fpsHistory = new Queue<double>();
        private const int fpsHistorySize = 30; // Average over 30 frames

        #region Configs
        private const int fps = 60;
        private const int screenWidth = 512;
        private const int screenHeight = 512;
        #endregion

        public LoopWindowTest2()
        {
            InitializeComponent();

            // Initialize the render target bitmap
            renderBitmap = new RenderTargetBitmap(screenWidth, screenHeight, 96, 96, PixelFormats.Pbgra32);
            theScreen.Source = renderBitmap;

            // Setup and start the timer
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000 / fps); // Approximately 60 FPS
            dispatcherTimer.Start();
        }

        public DrawingVisual GraphicScreen()
        {
            DrawingVisual dVisuals = new();

            BitmapSource bitmapSource = NoiseMethods.PlusSignNoiseImage(1024, 1024, 8);

            using (DrawingContext dContext = dVisuals.RenderOpen())
            {
                // Clear background
                dContext.DrawRectangle(ScreenBackground, null, new Rect(0, 0, screenWidth, screenHeight));

                //draw noise image on top of background
                dContext.DrawImage(bitmapSource, new Rect(0, 0, screenWidth, screenHeight));


                // Example drawing - you can add your custom drawing logic here
                //dContext.DrawRectangle(Brushes.Azure, null, new Rect(50, 50, 200, 200));
                //dContext.DrawRectangle(Brushes.Azure, null, RectiPecti());
                dContext.DrawEllipse(Brushes.Coral, new Pen(Brushes.DarkRed, 2), new System.Windows.Point(300, 300), 50, 50);

                // Draw FPS counter
                DrawFpsCounter(dContext);
            }
            return dVisuals;
        }

        private void DrawFpsCounter(DrawingContext dContext)
        {
            string fpsText = $"FPS: {currentFps:F1}";

            FormattedText formattedText = new FormattedText(
                fpsText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                20,
                Brushes.White,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            // Draw a semi-transparent background for better readability
            Rect textBounds = new Rect(5, 5, formattedText.Width + 10, formattedText.Height + 6);
            dContext.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0)), null, textBounds);

            // Draw the text
            dContext.DrawText(formattedText, new System.Windows.Point(10, 8));
        }

        #region Drawing Functions
        private Rect RectiPecti()
        {
            var random = new Random();
            var x = random.Next(0, screenWidth);
            var y = random.Next(0, screenHeight);
            var width = random.Next(0, screenWidth - x);
            var height = random.Next(0, screenHeight - y);
            var recti = new Rect(x, y, width, height);
            return recti;
        }
        #endregion

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            // Calculate FPS
            DateTime currentTime = DateTime.Now;
            double deltaTime = (currentTime - lastFrameTime).TotalSeconds;
            lastFrameTime = currentTime;

            if (deltaTime > 0)
            {
                double instantFps = 1.0 / deltaTime;
                fpsHistory.Enqueue(instantFps);

                // Keep only the last N frames
                if (fpsHistory.Count > fpsHistorySize)
                {
                    fpsHistory.Dequeue();
                }

                // Calculate average FPS
                currentFps = fpsHistory.Average();
            }

            // Create the visual scene
            DVisuals = GraphicScreen();

            // Render the visual to the bitmap
            if (renderBitmap != null && DVisuals != null)
            {
                renderBitmap.Clear();
                renderBitmap.Render(DVisuals);
            }
        }
    }
}

