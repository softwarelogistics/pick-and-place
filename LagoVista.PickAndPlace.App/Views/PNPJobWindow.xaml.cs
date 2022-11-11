using LagoVista.PickAndPlace.App.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using System.Diagnostics;
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
            this.KeyUp += PNPJobWindow_KeyUp;
        }

        private void PNPJobWindow_KeyUp(object sender, KeyEventArgs e)
        {
            ViewModel.MachineControls.HandleKeyDown((WindowsKey)e.Key, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift), Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
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

        private void ListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.PageUp || e.Key == Key.PageDown)
                e.Handled = true;
        }

        private void Grid_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void CheckBox_KeyUp(object sender, KeyEventArgs e)
        {
            var chk = sender as CheckBox;
            if (chk.IsChecked.Value)
            {
                e.Handled = true;
                //this.ViewModel.ToolAlignmentVM.SetTool1Location();
            }
        }
    }
}
