using BasicBitmapManipulation.DrawCommon;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BasicBitmapManipulation
{
    /// <summary>
    /// Interaction logic for LoopWindowTest4.xaml
    /// </summary>
    public partial class LoopWindowTest4 : Window
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

        // 3D Cube rotation tracking
        private double rotationAngleX = 0; // Rotation around X-axis
        private double rotationAngleY = 0; // Rotation around Y-axis
        private double rotationAngleZ = 0; // Rotation around Z-axis
        private double rotationSpeed = 30; // Rotation speed in degrees per second

        #region Configs
        private const int fps = 60;
        private const int screenWidth = 256;//480;
        private const int screenHeight = 160;//360;
        #endregion

        public LoopWindowTest4()
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

            BitmapSource bitmapSource = NoiseMethods.UniformRandomNoiseImage(screenWidth, screenHeight, 12);

            using (DrawingContext dContext = dVisuals.RenderOpen())
            {
                // Clear background
                dContext.DrawRectangle(ScreenBackground, null, new Rect(0, 0, screenWidth, screenHeight));
                //draw noise image on top image
                dContext.DrawImage(bitmapSource, new Rect(0, 0, screenWidth, screenHeight));

                DrawMovineLine(dContext);
                // Draw 3D cube
                //DrawCube(dContext);

                // Draw FPS counter
                CommonCustomDrawing.DrawFpsCounter(dContext, currentFps, this, screenWidth, screenHeight);
            }
            return dVisuals;
        }

        private static void DrawMovineLine(DrawingContext dContext)
        {
            int randomNum = GetRandomNumber(98);
            
            dContext.DrawLine(new Pen(Brushes.Blue, GetRandomNumber(3)), new Point(0, 0), new Point(randomNum, 100));
            dContext.DrawLine(new Pen(Brushes.Blue, GetRandomNumber(2)), new Point(0, 0), new Point(randomNum, 100));
            dContext.DrawLine(new Pen(Brushes.Blue, GetRandomNumber(1)), new Point(0, 0), new Point(randomNum, 100));
            dContext.DrawLine(new Pen(Brushes.Blue, GetRandomNumber(1)), new Point(0, 0), new Point(randomNum, 100));
        }

        private static int GetRandomNumber(int value = 1)
        {
            return new Random().Next(value);
        }

        private void DrawCube(DrawingContext dContext)
        {
            // Define cube size and center position
            double cubeSize = 64;
            double centerX = screenWidth / 2;
            double centerY = screenHeight / 2;

            // Define the 8 vertices of a cube (centered at origin)
            double[,] vertices = new double[8, 3]
            {
                { -1, -1, -1 },  // 0: back-bottom-left
                {  1, -1, -1 },  // 1: back-bottom-right
                {  1,  1, -1 },  // 2: back-top-right
                { -1,  1, -1 },  // 3: back-top-left
                { -1, -1,  1 },  // 4: front-bottom-left
                {  1, -1,  1 },  // 5: front-bottom-right
                {  1,  1,  1 },  // 6: front-top-right
                { -1,  1,  1 }   // 7: front-top-left
            };

            // Scale vertices by cube size
            for (int i = 0; i < 8; i++)
            {
                vertices[i, 0] *= cubeSize / 2;
                vertices[i, 1] *= cubeSize / 2;
                vertices[i, 2] *= cubeSize / 2;
            }

            // Rotate vertices
            double[,] rotatedVertices = new double[8, 3];
            for (int i = 0; i < 8; i++)
            {
                double x = vertices[i, 0];
                double y = vertices[i, 1];
                double z = vertices[i, 2];

                // Apply rotation around X, Y, and Z axes
                var rotated = RotatePoint(x, y, z, rotationAngleX, rotationAngleY, rotationAngleZ);
                rotatedVertices[i, 0] = rotated.x;
                rotatedVertices[i, 1] = rotated.y;
                rotatedVertices[i, 2] = rotated.z;
            }

            // Project 3D points to 2D
            Point[] projectedPoints = new Point[8];
            for (int i = 0; i < 8; i++)
            {
                projectedPoints[i] = Project3DTo2D(rotatedVertices[i, 0], rotatedVertices[i, 1], rotatedVertices[i, 2], centerX, centerY);
            }

            // Define the 12 edges of the cube
            int[,] edges = new int[12, 2]
            {
                { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 }, // Back face
                { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 4 }, // Front face
                { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }  // Connecting edges
            };

            // Draw the edges
            Pen cubePen = new Pen(Brushes.LightBlue, 2);
            for (int i = 0; i < 12; i++)
            {
                int v1 = edges[i, 0];
                int v2 = edges[i, 1];
                dContext.DrawLine(cubePen, projectedPoints[v1], projectedPoints[v2]);
            }
        }

        private (double x, double y, double z) RotatePoint(double x, double y, double z, double angleX, double angleY, double angleZ)
        {
            // Convert angles to radians
            double radX = angleX * Math.PI / 180.0;
            double radY = angleY * Math.PI / 180.0;
            double radZ = angleZ * Math.PI / 180.0;

            // Rotate around X-axis
            double y1 = y * Math.Cos(radX) - z * Math.Sin(radX);
            double z1 = y * Math.Sin(radX) + z * Math.Cos(radX);
            y = y1;
            z = z1;

            // Rotate around Y-axis
            double x1 = x * Math.Cos(radY) + z * Math.Sin(radY);
            z1 = -x * Math.Sin(radY) + z * Math.Cos(radY);
            x = x1;
            z = z1;

            // Rotate around Z-axis
            x1 = x * Math.Cos(radZ) - y * Math.Sin(radZ);
            y1 = x * Math.Sin(radZ) + y * Math.Cos(radZ);
            x = x1;
            y = y1;

            return (x, y, z);
        }

        private Point Project3DTo2D(double x, double y, double z, double centerX, double centerY)
        {
            // Simple orthographic projection
            // For perspective projection, you can use: scale = focalLength / (focalLength + z)
            double perspective = 200.0 / (200.0 + z);
            return new Point(centerX + x * perspective, centerY - y * perspective);
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

                // Update rotation angles for 3D cube
                rotationAngleX += rotationSpeed * deltaTime;
                rotationAngleY += rotationSpeed * deltaTime * 0.7;
                rotationAngleZ += rotationSpeed * deltaTime * 0.5;

                if (rotationAngleX >= 360) rotationAngleX -= 360;
                if (rotationAngleY >= 360) rotationAngleY -= 360;
                if (rotationAngleZ >= 360) rotationAngleZ -= 360;
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



