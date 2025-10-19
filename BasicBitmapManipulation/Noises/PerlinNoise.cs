using System.Numerics;

namespace BasicBitmapManipulation.Noises
{
    public class PerlinNoise
    {
        private static readonly int[] p = new int[512];
        private static readonly Vector3[] grad3 = new Vector3[16];

        static PerlinNoise()
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
            int X = (int)Math.Floor(x) & 255;
            int Y = (int)Math.Floor(y) & 255;
            x -= Math.Floor(x);
            y -= Math.Floor(y);

            double u = Fade(x);
            double v = Fade(y);

            int A = p[X] + Y;
            int AA = p[A];
            int AB = p[A + 1];
            int B = p[X + 1] + Y;
            int BA = p[B];
            int BB = p[B + 1];

            Vector3 gradA = grad3[AA & 15];
            Vector3 gradB = grad3[BA & 15];
            Vector3 gradC = grad3[AB & 15];
            Vector3 gradD = grad3[BB & 15];

            double n00 = Dot(gradA, x, y);
            double n01 = Dot(gradB, x - 1, y);
            double n10 = Dot(gradC, x, y - 1);
            double n11 = Dot(gradD, x - 1, y - 1);

            return Lerp(Lerp(n00, n10, u), Lerp(n01, n11, u), v);
        }

        private double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private double Lerp(double a, double b, double t)
        {
            return a + t * (b - a);
        }

        private double Dot(Vector3 grad, double x, double y)
        {
            return grad.X * x + grad.Y * y;
        }
    }
}
