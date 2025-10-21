using BasicBitmapManipulation.DrawCommon;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BasicBitmapManipulation.Windows
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
        
        // Y-axis rotation (3D depth effect)
        private double rotationAngleY = 0; // Rotation around Y-axis in degrees
        private double rotationSpeedY = 60; // Y-axis rotation speed in degrees per second

        #region Configs
        private const int fps = 60;
        private const int screenWidth = 480;
        private const int screenHeight = 360;
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

            BitmapSource bitmapSource = NoiseMethods.UniformRandomNoiseImage(480, 360, 12);

            using (DrawingContext dContext = dVisuals.RenderOpen())
            {
                // Clear background
                dContext.DrawRectangle(ScreenBackground, null, new Rect(0, 0, screenWidth, screenHeight));
                //draw noise image on top image
                dContext.DrawImage(bitmapSource, new Rect(0, 0, screenWidth, screenHeight));
                
                // Define a 1-pixel pen
                Pen outlinePen = new Pen(Brushes.Black, 1);

                // First rectangle: Z-axis rotation (2D rotation)
                Rect rectangle = new Rect(50, 50, 24, 24);
                Point rectangleCenter = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);

                // Apply rotation transformation around rectangle center
                dContext.PushTransform(new RotateTransform(rotationAngle, rectangleCenter.X, rectangleCenter.Y));
                
                // Draw the rotating rectangle
                dContext.DrawRectangle(null, outlinePen, rectangle);
                
                // Remove the transformation
                dContext.Pop();

                // Second rectangle: Y-axis rotation (3D depth effect)
                Rect rectangle2 = new Rect(250, 50, 16, 16);
                Point rectangle2Center = new Point(rectangle2.X + rectangle2.Width / 2, rectangle2.Y + rectangle2.Height / 2);

                // Calculate scale factor for Y-axis rotation (simulates 3D depth)
                double angleRadians = rotationAngleY * Math.PI / 180.0;
                double scaleX = Math.Cos(angleRadians);

                // Apply transformations: first scale (for 3D effect), then translate to center
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform(scaleX, 1, rectangle2Center.X, rectangle2Center.Y));
                
                dContext.PushTransform(transformGroup);
                
                // Draw the 3D rotating rectangle with red pen
                Pen redPen = new Pen(Brushes.Red, 1);
                dContext.DrawRectangle(null, redPen, rectangle2);
                
                // Remove the transformation
                dContext.Pop();

                //dContext.DrawEllipse(Brushes.Coral, new Pen(Brushes.DarkRed, 2), new System.Windows.Point(300, 300), 50, 50);

                // Draw FPS counter
                CommonCustomDrawing.DrawFpsCounter(dContext, currentFps, this, screenWidth, screenHeight);


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

                // Update Z-axis rotation angle (2D rotation)
                rotationAngle += rotationSpeed * deltaTime;
                if (rotationAngle >= 360)
                {
                    rotationAngle -= 360;
                }

                // Update Y-axis rotation angle (3D depth rotation)
                rotationAngleY += rotationSpeedY * deltaTime;
                if (rotationAngleY >= 360)
                {
                    rotationAngleY -= 360;
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

