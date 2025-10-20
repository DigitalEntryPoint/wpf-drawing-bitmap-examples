using System;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BasicBitmapManipulation.Noises
{
    public class SimplexNoise
    {
        private static readonly int[] p = new int[512];
        private static readonly Vector3[] grad3 = new Vector3[16];

        static SimplexNoise()
        {
            // Initialize permutation table
            for (int i = 0; i < 256; i++)
            {
                p[i] = i;
            }
            for (int i = 0; i < 256; i++)
            {
                int j = (int)(new Random().NextDouble() * 256);
                int temp = p[i];
                p[i] = p[j];
                p[j] = temp;
            }
            for (int i = 0; i < 256; i++)
            {
                p[i + 256] = p[i];
            }

            // Initialize gradients
            grad3[0] = new Vector3(1, 1, 0);
            grad3[1] = new Vector3(-1, 1, 0);
            grad3[2] = new Vector3(1, -1, 0);
            grad3[3] = new Vector3(-1, -1, 0);
            grad3[4] = new Vector3(1, 0, 1);
            grad3[5] = new Vector3(-1, 0, 1);
            grad3[6] = new Vector3(1, 0, -1);
            grad3[7] = new Vector3(-1, 0, -1);
            grad3[8] = new Vector3(0, 1, 1);
            grad3[9] = new Vector3(0, -1, 1);
            grad3[10] = new Vector3(0, 1, -1);
            grad3[11] = new Vector3(0, -1, -1);
            grad3[12] = new Vector3(1, 1, 0);
            grad3[13] = new Vector3(-1, 1, 0);
            grad3[14] = new Vector3(0, -1, 1);
            grad3[15] = new Vector3(0, -1, -1);
        }

        public double Noise(double x, double y)
        {
            // Simplex noise implementation
            // This is a simplified version and may need further refinement for full functionality
            int i = (int)x;
            int j = (int)y;
            double f = x - i;
            double g = y - j;

            double n0 = Grad(i, j, f, g);
            double n1 = Grad(i + 1, j, f - 1, g);
            double n2 = Grad(i, j + 1, f, g - 1);
            double n3 = Grad(i + 1, j + 1, f - 1, g - 1);

            return (n0 + n1 + n2 + n3) / 4.0;
        }

        private double Grad(int ix, int iy, double x, double y)
        {
            int hash = p[ix + p[iy]] % 16;
            Vector3 g = grad3[hash];
            return g.X * x + g.Y * y;
        }
    }
}
