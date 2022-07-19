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
        public static T[,] To2DArray<T>(this T[] input, int height, int width)
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
        /// Removes every other byte and every other row from the array, making it 4x smaller
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[,] QuarterArray(this byte[,] array)
        {
            var result = new byte[array.GetLength(0) / 2, array.GetLength(1) / 2];

            for (int i = 0; i < array.GetLength(0); i += 2)
            {
                for (int j = 0; j < array.GetLength(1); j += 2)
                {
                    result[i / 2, j / 2] = array[i, j];
                }
            }

            return result;
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

        /// <summary>
        /// Returns the maximum distance between two pairs of values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int MaximumAbsoluteDifference(int x1, int y1, int x2, int y2)
        {
            return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        }

        /// <summary>
        /// Returns the manhattan distance between two pairs of values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int ManhattanDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
        }
    }
}
