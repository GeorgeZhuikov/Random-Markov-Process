using Microsoft.Win32;
using Random_Markov_Process_Class_Library;
using Random_Markov_Process_WPF.Classes;
using Random_Markov_Process_WPF.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Random_Markov_Process_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _enableRpm;
        private RandomMarkovProcess _rmp = new RandomMarkovProcess();

        public MainWindow()
        {
            InitializeComponent();
            _rmp.SetStatus = SetStatus;
        }

        #region Private functions

        private void SetStatus(string status)
        {
            tblStatus.Dispatcher.Invoke(() => tblStatus.Text = status);
        }

        private void OpenImage()
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
            };
            if (ofd.ShowDialog() == true)
            {
                _enableRpm = false;
                Task.Run(() =>
                {
                    var maxStateCount = _rmp.GetMaxStateCount(ofd.FileName);
                    Dispatcher.Invoke(() =>
                    {
                        iudStates.Maximum = maxStateCount;
                        if (iudStates.Value > maxStateCount)
                            iudStates.Value = maxStateCount;
                        _enableRpm = true;
                    });
                });
                _rmp.SetOriginalImage(ofd.FileName);
                SetImages(true);
            }
        }

        private void Simulate()
        {
            if (_enableRpm)
            {
                _enableRpm = false;
                var stateCount = (int)iudStates.Value;
                Task.Run(() =>
                {
                    if (_rmp.Simulate(stateCount))
                        Dispatcher.Invoke(() => SetImages());
                    Dispatcher.Invoke(() => _enableRpm = true);
                });
            }
        }

        private void SetGruopBoxesSize()
        {
            try
            {
                var height = (wpnImages.ActualHeight - 1) / 2;
                var width = (wpnImages.ActualWidth - 1) / 3;

                foreach (Control child in wpnImages.Children)
                {
                    child.Height = height;
                    child.Width = width;
                }

                var left = width / 2;
                gbxACF.Margin = new Thickness(left, 0, 0, 0);
            }
            catch { }
        }

        private void SetImages(bool clear = false)
        {
            if (clear)
            {
                imgOriginal.Source = Helper.BitmapToImageSource(_rmp.OriginalImage);
                imgRealRMP.Source = null;
                imgArtificial.Source = null;
                imgRealAcf.Source = null;
                imgArtificialAcf.Source = null;
            }
            else
            {
                imgRealRMP.Source = Helper.BitmapToImageSource(_rmp.RealImage);
                imgArtificial.Source = Helper.BitmapToImageSource(_rmp.ArtificialImage);
                imgRealAcf.Source = Helper.BitmapToImageSource(_rmp.RealAcfImage);
                imgArtificialAcf.Source = Helper.BitmapToImageSource(_rmp.ArtificialAcfImage);
            }
        }

        #endregion

        #region Private MenuItem methods

        private void ShowReference(object sender)
        {
            MessageBox.Show(Properties.Resources.MenuItem_Reference,
                ((MenuItem)sender).Header.ToString(),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ShowAbout(object sender)
        {
            MessageBox.Show(Properties.Resources.MenuItem_About,
                ((MenuItem)sender).Header.ToString(),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ShowLegend(object sender)
        {
            MessageBox.Show(Properties.Resources.MenuItem_Legend,
                ((MenuItem)sender).Header.ToString(),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #endregion


        #region Events

        #region MenuItem

        private void MenuItem_Click_Open(object sender, RoutedEventArgs e)
        {
            OpenImage();
        }

        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_Click_PRI_TMH(object sender, RoutedEventArgs e)
        {
            try
            {
                var wnd = new WndTransitionMatrix() { Title = ((MenuItem)sender).Header.ToString() };
                wnd.SetTransitionMatrix(_rmp.RealImageHorisontalTransitionMatrix);
                wnd.Show();
            }
            catch { }
        }

        private void MenuItem_Click_PRI_TMV(object sender, RoutedEventArgs e)
        {
            try
            {
                var wnd = new WndTransitionMatrix() { Title = ((MenuItem)sender).Header.ToString() };
                wnd.SetTransitionMatrix(_rmp.RealImageVerticalTransitionMatrix);
                wnd.Show();
            }
            catch { }
        }

        private void MenuItem_Click_PRI_TM(object sender, RoutedEventArgs e)
        {
            try
            {
                var wnd = new WndTransitionMatrix() { Title = ((MenuItem)sender).Header.ToString() };
                wnd.SetTransitionMatrix(_rmp.RealImageTransitionProbabilityMatrix);
                wnd.Show();
            }
            catch { }
        }

        private void MenuItem_Click_AI_TMH(object sender, RoutedEventArgs e)
        {
            try
            {
                var wnd = new WndTransitionMatrix() { Title = ((MenuItem)sender).Header.ToString() };
                wnd.SetTransitionMatrix(_rmp.ArtificialImageHorisontalTransitionMatrix);
                wnd.Show();
            }
            catch { }
        }

        private void MenuItem_Click_AI_TMV(object sender, RoutedEventArgs e)
        {
            try
            {
                var wnd = new WndTransitionMatrix() { Title = ((MenuItem)sender).Header.ToString() };
                wnd.SetTransitionMatrix(_rmp.ArtificialImageVerticalTransitionMatrix);
                wnd.Show();
            }
            catch { }
        }

        private void MenuItem_Click_AI_TM(object sender, RoutedEventArgs e)
        {
            try
            {
                var wnd = new WndTransitionMatrix() { Title = ((MenuItem)sender).Header.ToString() };
                wnd.SetTransitionMatrix(_rmp.ArtificialImageTransitionProbabilityMatrix);
                wnd.Show();
            }
            catch { }
        }

        private void MenuItem_Click_Simulate(object sender, RoutedEventArgs e)
        {
            Simulate();
        }

        private void MenuItem_Click_Reference(object sender, RoutedEventArgs e)
        {
            ShowReference(sender);
        }

        private void MenuItem_Click_About(object sender, RoutedEventArgs e)
        {
            ShowAbout(sender);
        }

        private void MenuItem_Click_Legend(object sender, RoutedEventArgs e)
        {
            ShowLegend(sender);
        }

        #endregion

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetStatus("Максимальное количество состояний: " + iudStates.Maximum);
            iudStates.Focus();
        }

        private void splImages_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetGruopBoxesSize();
        }

        private void grdMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            wpnImages.Height = grdMain.ActualHeight - mnuMain.ActualHeight;
        }

        private void img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var img = (Image)sender;

            var wnd = new WndImage()
            {
                ImageSource = img.Source,
                Title = ((GroupBox)((Image)sender).Parent).Header.ToString()
            };

            if (img.Name == "imgRealAcf")
                wnd.Graph3D = _rmp.RealImageGraph3D;
            if (img.Name == "imgArtificialAcf")
                wnd.Graph3D = _rmp.ArtificialImageGraph3D;

            wnd.Show();
        }

        #endregion

    }
}
