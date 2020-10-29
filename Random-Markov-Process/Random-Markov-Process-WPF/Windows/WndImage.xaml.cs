using Random_Markov_Process_Class_Library.ACF;
using Random_Markov_Process_WPF.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
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
    /// Interaction logic for WndImage.xaml
    /// </summary>
    public partial class WndImage : Window
    {
        private bool _isBusy;
        private int _sleep = 10;
        private Point _latestMousePos;
        private Graph3D _graph3D;

        public ImageSource ImageSource { set { img.Source = value; } }
        public new string Title { set { base.Title = value; } }
        public Graph3D Graph3D
        {
            set
            {
                _graph3D = value;
            }
        }

        public WndImage()
        {
            InitializeComponent();
        }


        private void MenuItem_Click_Copy(object sender, RoutedEventArgs e)
        {
            Helper.CopyImageSourceToClipboard(img);
        }

        private void MenuItem_Click_Reset(object sender, RoutedEventArgs e)
        {
            if (_graph3D != null)
            {
                _graph3D.Size = new System.Drawing.Size((int)img.ActualWidth, (int)img.ActualHeight);
                _graph3D.HorizontalAngleGrad = _graph3D.DefaultHorizontalAngleGrad;
                _graph3D.VerticalAngleGrad = _graph3D.DefaultVerticalAngleGrad;
                _graph3D.Zoom = _graph3D.DefaultZoom;
                img.Source = Helper.BitmapToImageSource(_graph3D.DrawGraph());
            }
        }

        private void MenuItem_Click_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_graph3D != null)
            {
                img.CaptureMouse();
                _latestMousePos = e.GetPosition(img);
            }
        }

        private void img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_graph3D != null)
            {
                img.ReleaseMouseCapture();
                GC.Collect();
            }
        }

        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            if (_graph3D != null && img.IsMouseCaptured && !_isBusy)
            {
                _isBusy = true;

                var pos = e.GetPosition(img);

                var difX = (pos.X - _latestMousePos.X) / 2;
                var difY = (pos.Y - _latestMousePos.Y) / 2;

                _graph3D.HorizontalAngleGrad += difX;
                _graph3D.VerticalAngleGrad += difY;

                _latestMousePos = pos;

                img.Source = null;
                img.Source = Helper.BitmapToImageSource(_graph3D.DrawGraph());

                Thread.Sleep(_sleep);

                _isBusy = false;
            }
        }

        private void img_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mitReset.Visibility = _graph3D != null ? Visibility.Visible : Visibility.Collapsed;
            if (_graph3D != null)
            {
                _graph3D.Size = new System.Drawing.Size((int)img.ActualWidth, (int)img.ActualHeight);
                img.Source = null;
                img.Source = Helper.BitmapToImageSource(_graph3D.DrawGraph());
            }
        }

        private void img_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_graph3D != null && !_isBusy)
            {
                _isBusy = true;

                _graph3D.Zoom += e.Delta / (double)800;

                img.Source = null;
                img.Source = Helper.BitmapToImageSource(_graph3D.DrawGraph());

                Thread.Sleep(_sleep);

                _isBusy = false;
            }
        }

    }
}
