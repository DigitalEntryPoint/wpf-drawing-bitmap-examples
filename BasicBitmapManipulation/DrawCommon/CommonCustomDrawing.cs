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
        public static void DrawFpsCounter(DrawingContext dContext, double currentFps, Window window)
        {
            string fpsText = $"FPS: {currentFps:F1}";

            FormattedText formattedText = new FormattedText(
                fpsText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                20,
                Brushes.White,
                VisualTreeHelper.GetDpi(window).PixelsPerDip);

            // Draw a semi-transparent background for better readability
            Rect textBounds = new Rect(5, 5, formattedText.Width + 10, formattedText.Height + 6);
            dContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)), null, textBounds);

            // Draw the text
            dContext.DrawText(formattedText, new Point(10, 8));
        }
    }
}

