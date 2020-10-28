using System.Diagnostics;

namespace Random_Markov_Process_Class_Library.ACF
{
    [DebuggerDisplay("X = {X}, Y = {Y}, Z = {Z}")]
    internal class Point3D
    {
        internal double X { get; }
        internal double Y { get; }
        internal double Z { get; }

        internal Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    internal class Points3D
    {
        internal double[] X { get; }
        internal double[] Y { get; }
        internal double[,] Z { get; }
        internal int Length { get { return X.Length; } }

        internal Points3D(double[] x, double[] y, double[,] z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
