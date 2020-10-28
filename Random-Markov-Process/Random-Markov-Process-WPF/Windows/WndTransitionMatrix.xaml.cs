using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Random_Markov_Process_WPF.Windows
{
    /// <summary>
    /// Interaction logic for WndTransitionMatrix.xaml
    /// </summary>
    public partial class WndTransitionMatrix : Window
    {
        private string _text;

        public new string Title { set { base.Title = value; } }

        public WndTransitionMatrix()
        {
            InitializeComponent();
        }


        private void FillDgd(decimal[,] matrix)
        {
            var length = matrix.GetLength(0);
            _text = "";

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                    _text += FormatValue(matrix[i, j]);                
                _text += Environment.NewLine;
            }
            tbl.Text = _text;
        }

        private void FillDgd(object[,] matrix)
        {
            var length = matrix.GetLength(0);
            _text = "";

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    var zMatrix = (decimal[])matrix[i, j];
                    for (int k = 0; k < length; k++)
                        _text += FormatValue(zMatrix[k]);
                    _text += Environment.NewLine;
                }
            }
            tbl.Text = _text;
        }

        private string FormatValue(decimal value)
        {
            return string.Format("{0,9:0.00000} ", value);
        }


        public void SetTransitionMatrix(decimal[,] matrix)
        {
            FillDgd(matrix);
        }

        public void SetTransitionMatrix(object[,] matrix)
        {
            FillDgd(matrix);
        }


        private void MenuItem_Click_Copy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_text);
        }

        private void MenuItem_Click_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
