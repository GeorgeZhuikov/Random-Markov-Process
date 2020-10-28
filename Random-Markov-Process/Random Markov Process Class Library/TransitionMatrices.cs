using Random_Markov_Process_Class_Library.ACF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Random_Markov_Process_Class_Library
{
    internal class TransitionMatrices
    {
        private int _statesCount;
        private StatesValues _statesValues;
        private Random _rand = new Random();


        internal int Width { get; private set; }
        internal int Height { get; private set; }
        internal int[,] StateMatrix { get; private set; }
        internal decimal[] InitialMatrix { get; private set; }
        internal decimal[] HorisontalSideTransitionMatrix { get; private set; }
        internal decimal[] VerticalSideTransitionMatrix { get; private set; }
        internal decimal[,] HorisontalTransitionMatrix { get; private set; }
        internal decimal[,] VerticalTransitionMatrix { get; private set; }
        internal object[,] TransitionProbabilityMatrix { get; private set; }
        internal Points3D ACF { get; private set; }


        internal TransitionMatrices(StatesValues statesValues)
        {
            _statesValues = statesValues;
            _statesCount = statesValues.StateCount;
            InitializeInitialMatrix();
            InitializeTransitionMatrices();
        }


        #region Private methods

        private int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        private void InitializeInitialMatrix()
        {
            InitialMatrix = new decimal[_statesCount];
            var prob = (decimal)1 / _statesCount;
            for (int index = 0; index < _statesCount; index++)
                InitialMatrix[index] = prob;
        }

        private void InitializeTransitionMatrices()
        {
            HorisontalSideTransitionMatrix = new decimal[_statesCount];
            VerticalSideTransitionMatrix = new decimal[_statesCount];

            HorisontalTransitionMatrix = new decimal[_statesCount, _statesCount];
            VerticalTransitionMatrix = new decimal[_statesCount, _statesCount];

            TransitionProbabilityMatrix = new object[_statesCount, _statesCount];
        }

        private void CalculateTransitionMatrix(decimal[,] matrix, decimal[] side, TransitionMatrixType type)
        {
            int latest = -1;

            switch (type)
            {
                case TransitionMatrixType.Horizontal:
                    for (int y = 0; y < Height; y++)
                        for (int x = 0; x < Width; x++)
                        {
                            var state = StateMatrix[y, x];
                            if (latest != -1)
                                matrix[state, latest]++;
                            latest = state;
                        }
                    break;
                case TransitionMatrixType.Vertical:
                    for (int x = 0; x < Width; x++)
                        for (int y = 0; y < Height; y++)
                        {
                            var state = StateMatrix[y, x];
                            if (latest != -1)
                                matrix[state, latest]++;
                            latest = state;
                        }
                    break;
                default: break;
            }

            for (int x = 0; x < _statesCount; x++)
                for (int y = 0; y < _statesCount; y++)
                    side[x] += matrix[y, x];

            var total = (decimal)StateMatrix.Length - 1;

            for (int y = 0; y < _statesCount; y++)
            {
                for (int x = 0; x < _statesCount; x++)
                    matrix[x, y] /= total;

                side[y] /= total;
            }

        }

        private void CalculateTransitionProbabilityMatrix()
        {
            var p3 = new decimal[_statesCount, _statesCount];
            for (int row = 0; row < _statesCount; row++)
            {
                for (int y = 0; y < _statesCount; y++)
                {
                    decimal mult = 0;
                    for (int x = 0; x < _statesCount; x++)
                        mult += HorisontalTransitionMatrix[row, x] * VerticalTransitionMatrix[x, y];
                    p3[row, y] = mult;
                }
            }

            for (int y = 0; y < _statesCount; y++)
                for (int x = 0; x < _statesCount; x++)
                {
                    var matrix = new decimal[_statesCount];
                    for (int z = 0; z < _statesCount; z++)
                    {
                        var p = p3[x, z];
                        var mult = HorisontalTransitionMatrix[x, y] * VerticalTransitionMatrix[y, z];
                        matrix[z] = p == 0 ? 0 : mult / p;
                    }
                    TransitionProbabilityMatrix[y, x] = matrix;
                }
        }

        private void SetACF()
        {
            int window = 50;

            var xArray = new double[window];
            var yArray = new double[window];
            var zArray = new double[window, window];

            var xBegin = (Width - window) / 2;
            if (xBegin < 0) xBegin = 0;
            var xEnd = xBegin + window;

            var yBegin = (Height - window) / 2;
            if (yBegin < 0) yBegin = 0;
            var yEnd = yBegin + window;

            var xMid = xBegin + (xEnd - xBegin) / 2;
            var yMid = xBegin + (yEnd - yBegin) / 2;

            var acfMax = GetACFValue(xMid, yMid, xMid, yMid, window);//GetACFValue(xxStart, yyStart, xxEnd, yyEnd, (xxEnd - xxStart) / 2, (yyEnd - yyStart) / 2);
            int indexY = 0;
            for (int y = yBegin; y < yEnd; y++)
            {
                int indexX = 0;
                for (int x = xBegin; x < xEnd; x++)
                {
                    var acf = GetACFValue(x, y, xMid, yMid, window);
                    var z = acfMax == 0 ? 0 : acf / acfMax;

                    zArray[indexY, indexX++] = z;
                }
                yArray[indexY] = indexY;
                xArray[indexY] = indexY++;
            }

            ACF = new Points3D(xArray, yArray, zArray);
        }

        private double GetACFValue(int x1, int y1, int x2, int y2, int window)
        {
            double result = 0;

            for (int y = 0; y < window; y++)
            {
                for (int x = 0; x < window; x++)
                {
                    if (StateMatrix[Mod(y1 + y, Height), Mod(x1 + x, Width)] == StateMatrix[Mod(y2 + y, Height), Mod(x2 + x, Width)])
                        result++;
                }
            }

            return result;
        }

        private int GetRandomStateIndex(decimal[] matrix, int state)
        {
            var result = state;

            var rval = (decimal)_rand.NextDouble();
            if (rval > matrix[result % _statesCount])
                result++;

            return result;
        }

        #endregion


        #region Internal methods Set

        internal void InitializeStateMatrix(int width, int height)
        {
            Width = width;
            Height = height;
            StateMatrix = new int[Height, Width];
        }

        internal void SetStateMatrix(int x, int y, int state)
        {
            StateMatrix[y,x] = state;
        }

        internal void SetTransitionMatrices()
        {
            CalculateTransitionMatrix(HorisontalTransitionMatrix, HorisontalSideTransitionMatrix, TransitionMatrixType.Horizontal);
            CalculateTransitionMatrix(VerticalTransitionMatrix, VerticalSideTransitionMatrix, TransitionMatrixType.Vertical);
            CalculateTransitionProbabilityMatrix();

            SetACF();
        }

        #endregion

        #region Internal methods Generate

        internal StateValue GetInitialStateColor()
        {
            var state = GetRandomStateIndex(InitialMatrix, 0);

            return _statesValues.GetStateValue(state);
        }

        internal StateValue GetSideStateColor(int latestState, TransitionMatrixType type)
        {
            var matrix = HorisontalSideTransitionMatrix;
            switch (type)
            {
                case TransitionMatrixType.Horizontal:
                    matrix = HorisontalSideTransitionMatrix;
                    break;
                case TransitionMatrixType.Vertical:
                    matrix = VerticalSideTransitionMatrix;
                    break;
                default: break;
            }

            var state = GetRandomStateIndex(matrix, latestState);

            return _statesValues.GetStateValue(state);
        }

        internal StateValue GetTransitionStateColor(int x, int y)
        {
            var left = StateMatrix[y, x - 1];
            var top = StateMatrix[y - 1, x];
            var corner = StateMatrix[y - 1, x - 1];

            var matrix = (decimal[])TransitionProbabilityMatrix[left, top];

            var state = GetRandomStateIndex(matrix, corner);

            return _statesValues.GetStateValue(state);
        }

        #endregion
    }

    internal enum TransitionMatrixType
    {
        Vertical,
        Horizontal
    }
}
