using System;

namespace MatFlat
{
    internal static partial class OpenBlas
    {
        internal static int f2cmin(int val1, int val2) => Math.Min(val1, val2);
        internal static int f2cmax(int val1, int val2) => Math.Max(val1, val2);
        internal static double abs(double d) => Math.Abs(d);
        internal static double sqrt(double d) => Math.Sqrt(d);
        internal static double log(double d) => Math.Log(d);
        internal static bool lsame_(string a, string b) => a == b;

        internal static unsafe double d_sign(double* a, double* b)
        {
            var __a = *a;
            var __b = *b;
            return ((__b) >= 0 ? ((__a) >= 0 ? (__a) : -(__a)) : -((__a) >= 0 ? (__a) : -(__a)));
        }
    }
}
