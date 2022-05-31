namespace Retro_ML.Utils
{
    public static class MathUtils
    {
        public static int Squared(this int v) => v * v;
        /// <summary>
        /// Returns <paramref name="val"/> to the power of <paramref name="pow"/>, assuming both are positive.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="pow"></param>
        /// <returns></returns>
        public static ulong PosPow(this int val, int pow)
        {
            if (pow < 0) return 0;

            ulong result = 1;
            var v = (uint)val;
            var p = (uint)pow;
            while (p-- > 0)
            {
                result *= v;
            }

            return result;
        }
        public static double ApproximateSquareRoot(this double v) => Math.Pow(10, Math.Log10(v) / 2);
        public static double ApproximateSquareRoot(this int v) => Math.Pow(10, Math.Log10(v) / 2);

        /// <summary>
        /// Turns the given array into a 2D array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static T[,] To2DArray<T>(T[] input, int height, int width)
        {
            T[,] output = new T[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    output[i, j] = input[i * width + j];
                }
            }
            return output;
        }

        /// <summary>
        /// Modulo that always returns a positive value.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        public static int PosModulo(this int v, int mod)
        {
            return ((v % mod) + mod) % mod;
        }
    }
}
