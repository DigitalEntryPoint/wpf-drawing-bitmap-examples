using BasicBitmapManipulation.DrawCommon;
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

        // Rotation tracking
        private double rotationAngle = 0; // Current rotation angle in degrees
        private double rotationSpeed = 45; // Rotation speed in degrees per second (adjustable)

        #region Configs
        private const int fps = 60;
        private const int screenWidth = 480;
        private const int screenHeight = 256;
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

            BitmapSource bitmapSource = NoiseMethods.UniformRandomNoiseImage(480, 256, 16);

            using (DrawingContext dContext = dVisuals.RenderOpen())
            {
                // Clear background
                dContext.DrawRectangle(ScreenBackground, null, new Rect(0, 0, screenWidth, screenHeight));
                //draw noise image on top image
                dContext.DrawImage(bitmapSource, new Rect(0, 0, screenWidth, screenHeight));
                
                // Define a 1-pixel pen
                Pen outlinePen = new Pen(Brushes.Black, 1);

                // Define rectangle properties
                Rect rectangle = new Rect(50, 50, 200, 200);
                Point rectangleCenter = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);

                // Apply rotation transformation around rectangle center
                dContext.PushTransform(new RotateTransform(rotationAngle, rectangleCenter.X, rectangleCenter.Y));
                
                // Draw the rotating rectangle
                dContext.DrawRectangle(null, outlinePen, rectangle);
                
                // Remove the transformation
                dContext.Pop();

                //dContext.DrawEllipse(Brushes.Coral, new Pen(Brushes.DarkRed, 2), new System.Windows.Point(300, 300), 50, 50);

                // Draw FPS counter
                CommonCustomDrawing.DrawFpsCounter(dContext, currentFps, this);


            }
            return dVisuals;
        }

        #region Drawing Functions

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

                // Update rotation angle based on speed and delta time
                rotationAngle += rotationSpeed * deltaTime;
                
                // Keep angle in 0-360 range (optional, prevents overflow)
                if (rotationAngle >= 360)
                {
                    rotationAngle -= 360;
                }
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

