using System.Windows;
using System.Windows.Media;

namespace BasicBitmapManipulation.DrawCommon
{
    /// <summary>
    /// Common custom drawing methods that can be reused across different windows
    /// </summary>
    public static class CommonCustomDrawing
    {
        /// <summary>
        /// Draws an FPS counter in the top-left corner with a semi-transparent background
        /// </summary>
        /// <param name="dContext">The DrawingContext to draw on</param>
        /// <param name="currentFps">The current FPS value to display</param>
        /// <param name="window">The window instance for DPI calculation</param>
        /// <param name="screenWidth">Width of the screen/canvas</param>
        /// <param name="screenHeight">Height of the screen/canvas</param>
        public static void DrawFpsCounter(DrawingContext dContext, double currentFps, Window window, double screenWidth, double screenHeight)
        {
            // Calculate responsive font size (approximately 5% of the smaller screen dimension)
            double fontSize = Math.Min(screenWidth, screenHeight) * 0.05;
            fontSize = Math.Max(12, Math.Min(fontSize, 32)); // Clamp between 12 and 32
            
            string fpsText = $"FPS: {currentFps:F1}";

            FormattedText formattedText = new FormattedText(
                fpsText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                fontSize,
                Brushes.White,
                VisualTreeHelper.GetDpi(window).PixelsPerDip);

            // Calculate responsive padding
            double padding = fontSize * 0.3;
            double margin = fontSize * 0.25;

            // Draw a semi-transparent background for better readability
            Rect textBounds = new Rect(margin, margin, formattedText.Width + padding * 2, formattedText.Height + padding);
            dContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)), null, textBounds);

            // Draw the text
            dContext.DrawText(formattedText, new Point(margin + padding, margin + padding * 0.5));
        }
    }
}

