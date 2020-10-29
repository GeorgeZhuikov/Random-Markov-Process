using Random_Markov_Process_Class_Library.ACF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Random_Markov_Process_Class_Library
{
    public class RandomMarkovProcess
    {
        private int _stateCount;
        private StatesValues _statesValues;
        private IamgeRMP _realImageRMP;
        private IamgeRMP _artificialImageRMP;


        #region Properties

        public int StateCount
        {
            get
            {
                return _stateCount;
            }
            set
            {
                if (value < 2)
                    _stateCount = 2;
                else
                    _stateCount = value;
            }
        }
        public Bitmap OriginalImage { get; private set; }
        public Bitmap RealImage => _realImageRMP.Image;
        public Bitmap ArtificialImage => _artificialImageRMP.Image;
        public Bitmap RealAcfImage => _realImageRMP.ImageAcf;
        public Bitmap ArtificialAcfImage => _artificialImageRMP.ImageAcf;
        public decimal[,] RealImageHorisontalTransitionMatrix => _realImageRMP.TransitionMatrices.HorisontalTransitionMatrix;
        public decimal[,] RealImageVerticalTransitionMatrix => _realImageRMP.TransitionMatrices.VerticalTransitionMatrix;
        public object[,] RealImageTransitionProbabilityMatrix => _realImageRMP.TransitionMatrices.TransitionProbabilityMatrix;
        public Graph3D RealImageGraph3D => _realImageRMP.Graph3D;
        public decimal[,] ArtificialImageHorisontalTransitionMatrix => _artificialImageRMP.TransitionMatrices.HorisontalTransitionMatrix;
        public decimal[,] ArtificialImageVerticalTransitionMatrix => _artificialImageRMP.TransitionMatrices.VerticalTransitionMatrix;
        public object[,] ArtificialImageTransitionProbabilityMatrix => _artificialImageRMP.TransitionMatrices.TransitionProbabilityMatrix;
        public Graph3D ArtificialImageGraph3D => _artificialImageRMP.Graph3D;
        public Action<string> SetStatus { get; set; }

        #endregion
        public RandomMarkovProcess()
        {
            StateCount = 2;
        }

        #region Private methods

        private void Proceed()
        {
            SetStatus?.Invoke("Формирование состояний...");
            _statesValues.StateCount = _stateCount;

            SetStatus?.Invoke("Преобразование реального изображения, расчёт матриц переходов и АКФ...");
            _realImageRMP = new IamgeRMP(_statesValues, OriginalImage);

            SetStatus?.Invoke("Генерация искусственного изображения, расчёт матриц переходов и АКФ...");
            _artificialImageRMP = new IamgeRMP(_statesValues, _realImageRMP.TransitionMatrices);
        }

        #endregion

        #region Public methods

        public bool Simulate()
        {
            var result = false;

            //try
            //{

                Proceed();
                result = true;

                SetStatus?.Invoke("Моделирование завершено");
            //}
            //catch
            //{
            //    SetStatus?.Invoke("Возникла ошибка при моделировании");
            //}

            return result;
        }

        public bool Simulate(int stateCount)
        {
            StateCount = stateCount;
            return Simulate();
        }

        public bool Simulate(string path)
        {
            OriginalImage = new Bitmap(path);
            return Simulate();
        }

        public bool Simulate(int states, string path)
        {
            StateCount = states;
            OriginalImage = new Bitmap(path);
            return Simulate();
        }

        public void SetOriginalImage(string path)
        {
            OriginalImage = new Bitmap(path);
        }

        public int GetMaxStateCount(string path)
        {
            SetStatus?.Invoke("Расчёт макимального количества состояний...");

            var bmp = new Bitmap(path);
            var stateValues = new List<StateValue>();

            int index = 0, maxStates = 20000;
            var values = new int[maxStates];
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var pixel = bmp.GetPixel(x, y);
                    var val = StatesValues.GetColorValue(pixel);
                    if (!values.Contains(val))
                    {
                        values[index] = val;
                        stateValues.Add(new StateValue(index++, val, pixel));
                    }
                    if (maxStates == index) break;
                }
                if (maxStates == index) break;
            }

            _statesValues = new StatesValues(stateValues);

            SetStatus?.Invoke("Максимальное количество состояний: " + stateValues.Count.ToString());

            return stateValues.Count;
        }

        #endregion

    }
}
