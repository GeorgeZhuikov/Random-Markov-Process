using Microsoft.CSharp.RuntimeBinder;
using Random_Markov_Process_Class_Library.ACF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace Random_Markov_Process_Class_Library
{
    internal class IamgeRMP
    {
        private StatesValues _statesValues;
        internal TransitionMatrices _transitionMatrices;
        private Bitmap _bmpOriginal;
        private Graph3D _graph3D;

        internal Bitmap Image { get; private set; }
        internal Bitmap ImageAcf { get; private set; }
        internal TransitionMatrices TransitionMatrices { get; private set; }
        internal Size Size { get { return _bmpOriginal.Size; } }
        internal Graph3D Graph3D => _graph3D;

        internal IamgeRMP(StatesValues statesValues, Bitmap bitmap)
        {
            Initialize();
            _statesValues = statesValues;
            _bmpOriginal = bitmap;
            TransitionMatrices = new TransitionMatrices(statesValues);
            ProceedReal();
        }

        internal IamgeRMP(StatesValues statesValues, TransitionMatrices transitionMatrices)
        {
            Initialize();
            _statesValues = statesValues;
            _transitionMatrices = transitionMatrices;
            Image = new Bitmap(_transitionMatrices.Width, _transitionMatrices.Height);
            TransitionMatrices = new TransitionMatrices(statesValues);
            ProceedArtificial();
        }


        private void Initialize()
        {
            _graph3D = new Graph3D();
        }

        private void ProceedReal()
        {
            TransitionMatrices.InitializeStateMatrix(_bmpOriginal.Width, _bmpOriginal.Height);

            Image = new Bitmap(_bmpOriginal.Width, _bmpOriginal.Height);
            for (int y = 0; y < _bmpOriginal.Height; y++)
                for (int x = 0; x < _bmpOriginal.Width; x++)
                {
                    var pixel = _bmpOriginal.GetPixel(x, y);
                    var sv = _statesValues.GetStateValue(pixel);
                    Image.SetPixel(x, y, sv.Color);
                    TransitionMatrices.SetStateMatrix(x, y, sv.ID);
                }

            TransitionMatrices.SetTransitionMatrices();
            
            _graph3D.Points3D = TransitionMatrices.ACF;
            ImageAcf = _graph3D.DrawGraph();
        }

        private void ProceedArtificial()
        {
            TransitionMatrices.InitializeStateMatrix(Image.Width, Image.Height);

            var init = _transitionMatrices.GetInitialStateColor();
            Image.SetPixel(0, 0, init.Color);
            TransitionMatrices.SetStateMatrix(0, 0, init.ID);

            Thread.Sleep(10);
            var latestValue = init.Value;
            for (int x = 1; x < Image.Width; x++)
            {
                var side = _transitionMatrices.GetSideStateColor(latestValue, TransitionMatrixType.Horizontal);
                Image.SetPixel(x, 0, side.Color);
                TransitionMatrices.SetStateMatrix(x, 0, side.ID);
                latestValue = side.Value;
            }

            Thread.Sleep(10);
            latestValue = init.Value;
            for (int y = 1; y < Image.Height; y++)
            {
                var side = _transitionMatrices.GetSideStateColor(latestValue, TransitionMatrixType.Vertical);
                Image.SetPixel(0, y, side.Color);
                TransitionMatrices.SetStateMatrix(0, y, side.ID);
                latestValue = side.Value;
            }

            for (int y = 1; y < Image.Height; y++)
            {
            Thread.Sleep(1);
                for (int x = 1; x < Image.Width; x++)
                {
                    var main = _transitionMatrices.GetTransitionStateColor(x, y);
                    Image.SetPixel(x, y, main.Color);
                    TransitionMatrices.SetStateMatrix(x, y, main.ID);
                }
            }

            TransitionMatrices.SetTransitionMatrices();

            _graph3D.Points3D = TransitionMatrices.ACF;
            ImageAcf = _graph3D.DrawGraph();
        }


    }
}
