namespace Retro_ML.Utils
{
    public static class MathUtils
    {
        public static int Squared(this int v) => v * v;
        public static double ApproximateSquareRoot(this double v) => Math.Pow(10, Math.Log10(v) / 2);
        public static double ApproximateSquareRoot(this int v) => Math.Pow(10, Math.Log10(v) / 2);
    }
}
