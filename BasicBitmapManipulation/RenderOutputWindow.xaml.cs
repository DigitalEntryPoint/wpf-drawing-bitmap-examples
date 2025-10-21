using BasicBitmapManipulation.DrawCommon;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BasicBitmapManipulation
{
    /// <summary>
    /// Base window for rendering graphics with FPS tracking
    /// </summary>
    public partial class RenderOutputWindow : Window
    {
        #region Properties
        public DrawingVisual? DVisuals { get; set; }
        public DrawingContext? DContext { get; set; }
        public static Brush ScreenBackground { get; set; } = Brushes.Transparent;
        protected RenderTargetBitmap? renderBitmap;
        #endregion

        #region FPS Tracking
        protected DateTime lastFrameTime = DateTime.Now;
        protected double currentFps = 0;
        protected Queue<double> fpsHistory = new Queue<double>();
        protected const int fpsHistorySize = 30; // Average over 30 frames
        #endregion

        #region Configuration
        protected const int fps = 60;
        protected int screenWidth = 360;
        protected int screenHeight = 256;
        #endregion

        #region Timer
        protected DispatcherTimer? dispatcherTimer;
        #endregion

        public RenderOutputWindow()
        {
            InitializeComponent();
            
            // Initialize with default settings
            InitializeRenderSystem(screenWidth, screenHeight, fps);
        }

        /// <summary>
        /// Initialize the rendering system with specified dimensions
        /// </summary>
        protected void InitializeRenderSystem(int width, int height, int targetFps = 60)
        {
            screenWidth = width;
            screenHeight = height;

            // Initialize the render target bitmap
            renderBitmap = new RenderTargetBitmap(screenWidth, screenHeight, 96, 96, PixelFormats.Pbgra32);
            theScreen.Source = renderBitmap;

            // Setup and start the timer
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / targetFps);
            dispatcherTimer.Start();
        }

        /// <summary>
        /// Override this method to create your custom graphics
        /// </summary>
        protected virtual DrawingVisual GraphicScreen()
        {
            DrawingVisual dVisuals = new();

            using (DrawingContext dContext = dVisuals.RenderOpen())
            {
                // Clear background
                dContext.DrawRectangle(ScreenBackground, null, new Rect(0, 0, screenWidth, screenHeight));

                // Draw FPS counter
                CommonCustomDrawing.DrawFpsCounter(dContext, currentFps, this, screenWidth, screenHeight);
            }

            return dVisuals;
        }

        /// <summary>
        /// Main render loop - called every frame
        /// </summary>
        protected virtual void DispatcherTimer_Tick(object? sender, EventArgs e)
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

            // Update frame logic
            UpdateFrame(deltaTime);

            // Create the visual scene
            DVisuals = GraphicScreen();

            // Render the visual to the bitmap
            if (renderBitmap != null && DVisuals != null)
            {
                renderBitmap.Clear();
                renderBitmap.Render(DVisuals);
            }
        }

        /// <summary>
        /// Override this to add per-frame update logic (animations, physics, etc.)
        /// </summary>
        protected virtual void UpdateFrame(double deltaTime)
        {
            // Override in derived classes for custom update logic
        }
    }
}

