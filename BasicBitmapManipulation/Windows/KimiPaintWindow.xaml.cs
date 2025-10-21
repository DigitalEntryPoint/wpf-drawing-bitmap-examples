using BasicBitmapManipulation.Extensions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BasicBitmapManipulation.Windows
{
   
    public partial class KimiPaintWindow : Window
    {
        private WriteableBitmap bitmap;
        private Point? previousPoint = null;
        private Point? previousScaledPoint = null;

        public string Status1 { get; set; }
        public string Status2 { get; set; }

        public KimiPaintWindow()
        {
            InitializeComponent();

            //add mouse events to image
            image.MouseDown += Image_MouseDown;
            image.MouseMove += Image_MouseMove;
            image.MouseUp += Image_MouseUp;

            bitmap = new WriteableBitmap(560, 560, 96, 96, PixelFormats.Bgra32, null);

            //fill bitmap with white color
            bitmap.Fill(Colors.White);


            image.Source = bitmap;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {           
            previousScaledPoint = PointOnStrechedImage(e.GetPosition(image));
            DrawPoint(previousScaledPoint.Value);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {

            UpdateStatus(e.GetPosition(image));

            // مختصات موس نسبت به کنترل Image
            Point currentPoint = e.GetPosition(image);
            Point scaledcurrentPoint = PointOnStrechedImage(currentPoint);
            UpdateStatus2(scaledcurrentPoint);

            if (previousScaledPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                DrawLine(previousScaledPoint.Value, scaledcurrentPoint);
                //previousPoint = scaledcurrentPoint;
                previousScaledPoint = scaledcurrentPoint;
            }
        }

        private Point PointOnStrechedImage(Point currentPoint)
        {
            // در نظر گرفتن مقیاس تصویر
            double scaleX = image.Source.Width / image.RenderSize.Width;
            double scaleY = image.Source.Height / image.RenderSize.Height;

            // تبدیل مختصات موس به مختصات تصویر اصلی
            Point scaledPoint = new Point(currentPoint.X * scaleX, currentPoint.Y * scaleY);
            return scaledPoint;
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            previousScaledPoint = null;
        }

        private void DrawPoint(Point point)
        {
            int x = (int)point.X;
            int y = (int)point.Y;
            bitmap.SetPixel(x, y, Colors.Black);
        }

        private void DrawLine(Point start, Point end)
        {
            int x0 = (int)start.X;
            int y0 = (int)start.Y;
            int x1 = (int)end.X;
            int y1 = (int)end.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                bitmap.SetPixel(x0, y0, Colors.Black);

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        #region feedback items
        //add method that updates status text
        private void UpdateStatus(Point point1)
        {
            statusText1.Text = $"X:{point1.X},Y:{point1.Y}";
            //statusText2.Text = Status2;
        }
        private void UpdateStatus2(Point point1)
        {
            //format double to 2 decimal point
            statusText2.Text = $"X:{point1.X.ToString("F2")},Y:{point1.Y.ToString("F2")}";
             
        }
        #endregion
    }

}
