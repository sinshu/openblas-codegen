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

        internal static unsafe double d_sign(double* a, double* b)
        {
            var __a = *a;
            var __b = *b;
            return ((__b) >= 0 ? ((__a) >= 0 ? (__a) : -(__a)) : -((__a) >= 0 ? (__a) : -(__a)));
        }

        internal static bool lsame_(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            return a.SequenceEqual(b);
        }

        internal static unsafe void s_cat(byte* lpp, AddressArray2 rpp, int* rnp, int* np, int llp)
        {
            throw new NotImplementedException();
        }



        internal ref struct AddressArray2
        {
            private ReadOnlySpan<byte> item0;
            private ReadOnlySpan<byte> item1;

            public ReadOnlySpan<byte> this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0: return item0;
                        case 1: return item1;
                        default: throw new IndexOutOfRangeException();
                    }
                }

                set
                {
                    switch (index)
                    {
                        case 0: item0 = value; break;
                        case 1: item1 = value; break;
                        default: throw new IndexOutOfRangeException();
                    }
                }
            }
        }
    }
}
