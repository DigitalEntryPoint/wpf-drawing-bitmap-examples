using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BasicBitmapManipulation.Extensions
{
    public static class WriteableBitmapExtensions
    {
        public static void SetPixel(this WriteableBitmap bitmap, int x, int y, Color color)
        {
            if (x < 0 || x >= bitmap.PixelWidth || y < 0 || y >= bitmap.PixelHeight)
                return;

            int index = y * bitmap.PixelWidth + x;
            int stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);
            int offset = index * (bitmap.Format.BitsPerPixel / 8);

            bitmap.Lock();
            nint backBuffer = bitmap.BackBuffer;

            unsafe
            {
                byte* p = (byte*)backBuffer + offset;
                p[0] = color.B;
                p[1] = color.G;
                p[2] = color.R;
                p[3] = color.A;
            }

            bitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            bitmap.Unlock();
        }

        //copilot
        public static void Fill(this WriteableBitmap bitmap, Color color)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[height * stride];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = color.B;
                pixels[i + 1] = color.G;
                pixels[i + 2] = color.R;
                pixels[i + 3] = color.A;
            }

            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }
    }
     
}
