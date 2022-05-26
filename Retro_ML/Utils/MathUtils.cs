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
    }
}
