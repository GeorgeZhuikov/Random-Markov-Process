using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Random_Markov_Process_Class_Library
{
    [DebuggerDisplay("ID = {ID}, Value = {Value}, Color = {Color.Name}")]
    public struct StateValue
    {
        private int _id;

        public int ID => _id;
        public int Value { get; }
        public Color Color { get; }
        
        public StateValue(int id, int value, Color color)
        {
            _id = id;
            Value = value;
            Color = color;
        }

        internal void SetID(int id)
        {
            _id = id;
        }
    }

    public class StatesValues
    {
        private int _statesCount;
        private List<StateValue> _stateValuesTotal;
        private StateValue[] _stateValues;

        internal int StateCount
        {
            get { return _statesCount; }
            set
            {
                _statesCount = value;
                SelectStateValues(value);
            }
        }


        internal StatesValues(List<StateValue> stateValues)
        {
            _stateValuesTotal = stateValues;          
        }


        private void SelectStateValues(int stateCount)
        {
            var step = _stateValuesTotal.Count / (stateCount - 1) - 1;

            var newStateValues = new List<StateValue>();
            var id = 0;
            for (int index = 0; index < _stateValuesTotal.Count; index += step)
            {
                var sv = _stateValuesTotal[index];
                sv.SetID(id++);
                newStateValues.Add(sv);
            }

            _stateValues = newStateValues.ToArray();
        }


        internal static int GetColorValue(Color color)
        {
            return color.R + color.G * 256 + color.B * 65536;
        }

        internal StateValue GetStateValue(Color color)
        {
            StateValue result = new StateValue();

            var val = GetColorValue(color);
            int min = int.MaxValue;
            for (int index = 0; index < StateCount; index++)
            {
                var sv = _stateValues[index];
                var dif = Math.Abs(val - sv.Value);
                if (dif < min)
                {
                    min = dif;
                    result = sv;
                }
            }

            return result;
        }

        internal StateValue GetStateValue(int index)
        {
            return _stateValues[index % StateCount];
        }
    }
}
