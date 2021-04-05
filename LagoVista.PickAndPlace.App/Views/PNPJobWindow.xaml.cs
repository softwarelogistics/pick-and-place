using LagoVista.PickAndPlace.App.ViewModels;
using LagoVista.PickAndPlace.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LagoVista.PickAndPlace.App.Views
{
    /// <summary>
    /// Interaction logic for PNPJob.xaml
    /// </summary>
    public partial class PNPJobWindow : Window
    {
        public PNPJobWindow()
        {
            InitializeComponent();
            this.Closing += PNPJobWindow_Closing;
        }

        private void PNPJobWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.StopCapture();
        }

        public PnPJobViewModel ViewModel
        {
            get => DataContext as PnPJobViewModel;
        }

        private void WebCamImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var percentX = (e.GetPosition(WebCamImage).X / WebCamImage.ActualWidth) - 0.5;
            var percentY = -((e.GetPosition(WebCamImage).Y / WebCamImage.ActualHeight) - 0.5);
            var absX = percentX * 38 + ViewModel.Machine.MachinePosition.X;
            var absY = percentY * 28 + ViewModel.Machine.MachinePosition.Y;

            ViewModel.Machine.GotoPoint(absX, absY);
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var placeablePart = e.Row.DataContext as PlaceableParts;
            if (placeablePart != null && placeablePart.PartStrip != null)
            {
                if (placeablePart.PartStrip.AvailablePartCount >= placeablePart.Count)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Green);
                    e.Row.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    e.Row.Background = new SolidColorBrush(Colors.Yellow);
                }
            }
        }

        private void WebCamImage2_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
