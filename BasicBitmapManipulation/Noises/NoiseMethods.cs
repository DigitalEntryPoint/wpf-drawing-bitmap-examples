using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BasicBitmapManipulation.Noises
{
    /// <summary>
    /// Static class containing various noise generation methods for creating BitmapSource textures
    /// </summary>
    public static class NoiseMethods
    {
        /// <summary>
        /// Generates basic random noise using NextBytes
        /// </summary>
        public static BitmapSource NoiseImage(int noiseWidth = 256, int noiseHeight = 256)
        {
            var random = new Random();
            var pixels = new byte[noiseWidth * noiseHeight * 4];
            random.NextBytes(pixels);
            BitmapSource bitmapSource = BitmapSource.Create(noiseWidth, noiseHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, noiseWidth * 4);
            return bitmapSource;
        }

        /// <summary>
        /// Generates uniform random noise with controllable alpha channel
        /// </summary>
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

        /// <summary>
        /// Generates smooth Perlin noise - ideal for organic textures, clouds, terrain
        /// </summary>
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

        /// <summary>
        /// Generates Gaussian (normal distribution) noise
        /// </summary>
        public static BitmapSource GaussianNoiseImage(int noiseWidth = 256, int noiseHeight = 256, byte alpha = 255)
        {
            var random = new Random();
            var pixels = new byte[noiseWidth * noiseHeight * 4];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                // Generate Gaussian noise values using Box-Muller transform
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

        /// <summary>
        /// Generates Simplex noise - similar to Perlin but with better performance
        /// </summary>
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

        /// <summary>
        /// Generates a pattern of randomly placed plus signs
        /// </summary>
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

        /// <summary>
        /// Helper method to draw a single plus sign on a pixel array
        /// </summary>
        private static void DrawPlusSign(byte[] pixels, int x, int y, int size, byte alpha, int width)
        {
            int halfSize = size / 2;

            // Draw the horizontal line of the plus sign
            for (int i = 0; i < size; i++)
            {
                int index = ((y + halfSize) * width + x + i) * 4;
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
                int index = ((y + i) * width + x + halfSize) * 4;
                if (index + 3 < pixels.Length)
                {
                    pixels[index] = 255;       // Blue
                    pixels[index + 1] = 255;   // Green
                    pixels[index + 2] = 255;   // Red
                    pixels[index + 3] = alpha; // Alpha
                }
            }
        }
    }
}

