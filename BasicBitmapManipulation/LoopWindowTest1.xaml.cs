using BasicBitmapManipulation.Noises;
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

            BitmapSource bitmapSource = PlusSignNoiseImage(1024,1024,8);

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

#region Noise Images
        private static BitmapSource NoiseImage(int noiseWidth=256,int noiseHeight=256)
        {
            //noise image            
            var random = new Random();
            var pixels = new byte[noiseWidth * noiseHeight * 4];
            random.NextBytes(pixels);
            BitmapSource bitmapSource = BitmapSource.Create(noiseWidth, noiseHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, noiseWidth * 4);
            return bitmapSource;
        }

        public static BitmapSource UniformRandomNoiseImage(int noiseWidth = 256, int noiseHeight = 256, byte alpha = 255)
        {
            var random = new Random();
            var pixels = new byte[noiseWidth * noiseHeight * 4];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = (byte)random.Next(256);       // Blue
                pixels[i + 1] = (byte)random.Next(256);   // Green
                pixels[i + 2] = (byte)random.Next(256);   // Red
                pixels[i + 3] = alpha;                    // Alpha
            }

            return BitmapSource.Create(noiseWidth, noiseHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, noiseWidth * 4);
        }

        public static BitmapSource PerlinNoiseImage(int noiseWidth = 256, int noiseHeight = 256, byte alpha = 255)
        {
            var pixels = new byte[noiseWidth * noiseHeight * 4];
            var perlinNoise = new PerlinNoise();

            for (int y = 0; y < noiseHeight; y++)
            {
                for (int x = 0; x < noiseWidth; x++)
                {
                    // Generate Perlin noise value
                    double noiseValue = perlinNoise.Noise(x / 100.0, y / 100.0);

                    // Normalize noise value to 0-255 range
                    byte grayValue = (byte)((noiseValue + 1) * 0.5 * 255);

                    // Calculate index in pixel array
                    int index = (y * noiseWidth + x) * 4;

                    // Set RGB values to the same gray value for grayscale noise
                    pixels[index] = grayValue;       // Blue
                    pixels[index + 1] = grayValue;   // Green
                    pixels[index + 2] = grayValue;   // Red
                    pixels[index + 3] = alpha;       // Alpha
                }
            }

            return BitmapSource.Create(noiseWidth, noiseHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, noiseWidth * 4);
        }

        public static BitmapSource GaussianNoiseImage(int noiseWidth = 256, int noiseHeight = 256, byte alpha = 255)
        {
            var random = new Random();
            var pixels = new byte[noiseWidth * noiseHeight * 4];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                // Generate Gaussian noise values
                double u1 = 1.0 - random.NextDouble();
                double u2 = 1.0 - random.NextDouble();
                double z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                double z1 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

                // Scale to 0-255 range
                byte grayValue = (byte)(Math.Abs(z0) * 255);

                pixels[i] = grayValue;       // Blue
                pixels[i + 1] = grayValue;   // Green
                pixels[i + 2] = grayValue;   // Red
                pixels[i + 3] = alpha;       // Alpha
            }

            return BitmapSource.Create(noiseWidth, noiseHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, noiseWidth * 4);
        }

        public static BitmapSource SimplexNoiseImage(int noiseWidth = 256, int noiseHeight = 256, byte alpha = 255)
        {
            var pixels = new byte[noiseWidth * noiseHeight * 4];
            var simplexNoise = new SimplexNoise();

            for (int y = 0; y < noiseHeight; y++)
            {
                for (int x = 0; x < noiseWidth; x++)
                {
                    // Generate Simplex noise value
                    double noiseValue = simplexNoise.Noise(x / 100.0, y / 100.0);

                    // Normalize noise value to 0-255 range
                    byte grayValue = (byte)((noiseValue + 1) * 0.5 * 255);

                    // Calculate index in pixel array
                    int index = (y * noiseWidth + x) * 4;

                    // Set RGB values to the same gray value for grayscale noise
                    pixels[index] = grayValue;       // Blue
                    pixels[index + 1] = grayValue;   // Green
                    pixels[index + 2] = grayValue;   // Red
                    pixels[index + 3] = alpha;       // Alpha
                }
            }

            return BitmapSource.Create(noiseWidth, noiseHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, noiseWidth * 4);
        }

        public static BitmapSource PlusSignNoiseImage(int noiseWidth = 256, int noiseHeight = 256, byte alpha = 255)
        {
            var random = new Random();
            var pixels = new byte[noiseWidth * noiseHeight * 4];

            // Clear the pixel array
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = 0;
            }

            // Calculate the number of plus signs that can fit in the bitmap
            int plusSize = 5;
            int numPlusX = noiseWidth / plusSize;
            int numPlusY = noiseHeight / plusSize;

            // Randomly place plus signs
            for (int i = 0; i < numPlusX * numPlusY; i++)
            {
                int x = random.Next(0, noiseWidth - plusSize);
                int y = random.Next(0, noiseHeight - plusSize);

                // Draw a plus sign centered at (x, y)
                DrawPlusSign(pixels, x, y, plusSize, alpha, noiseWidth);
            }

            return BitmapSource.Create(noiseWidth, noiseHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, noiseWidth * 4);
        }

        private static void DrawPlusSign(byte[] pixels, int x, int y, int size, byte alpha, int width)
        {
            int halfSize = size / 2;

            // Draw the horizontal line of the plus sign
            for (int i = 0; i < size; i++)
            {
                int index = ((y + halfSize) * width + (x + i)) * 4;
                if (index + 3 < pixels.Length)
                {
                    pixels[index] = 255;       // Blue
                    pixels[index + 1] = 255;   // Green
                    pixels[index + 2] = 255;   // Red
                    pixels[index + 3] = alpha; // Alpha
                }
            }

            // Draw the vertical line of the plus sign
            for (int i = 0; i < size; i++)
            {
                int index = ((y + i) * width + (x + halfSize)) * 4;
                if (index + 3 < pixels.Length)
                {
                    pixels[index] = 255;       // Blue
                    pixels[index + 1] = 255;   // Green
                    pixels[index + 2] = 255;   // Red
                    pixels[index + 3] = alpha; // Alpha
                }
            }
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
