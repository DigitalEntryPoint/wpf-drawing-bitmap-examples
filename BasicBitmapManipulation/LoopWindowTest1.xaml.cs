using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BasicBitmapManipulation
{
    /// <summary>
    /// Interaction logic for LoopWindowTest1.xaml
    /// </summary>
    public partial class LoopWindowTest1 : Window
    {
        public DrawingVisual? DVisuals { get; set; }
        public DrawingContext? DContext { get; set; }
        public static Brush ScreenBackground { get; set; } = Brushes.Transparent;
        private RenderTargetBitmap? renderBitmap;

        #region Configs
        private const int fps = 60;
        private const int screenWidth = 512;
        private const int screenHeight = 512;
        #endregion

        public LoopWindowTest1()
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

            //noise image
            int noiseWidth= 512;
            int noiseHeight= 512;
            var random = new Random();
            var pixels = new byte[noiseWidth * noiseHeight * 4];
            random.NextBytes(pixels);
            BitmapSource bitmapSource = BitmapSource.Create(noiseWidth, noiseHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, noiseWidth * 4);

            using (DrawingContext dContext = dVisuals.RenderOpen())
            {
                // Clear background
                dContext.DrawRectangle(ScreenBackground, null, new Rect(0, 0, screenWidth, screenHeight));

                //draw noise image on top of background
                dContext.DrawImage(bitmapSource, new Rect(0, 0, noiseWidth, noiseHeight));


                // Example drawing - you can add your custom drawing logic here
                dContext.DrawRectangle(Brushes.Azure, null, new Rect(50, 50, 200, 200));
                dContext.DrawEllipse(Brushes.Coral, new Pen(Brushes.DarkRed, 2), new System.Windows.Point(300, 300), 50, 50);
            }
            return dVisuals;
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
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
