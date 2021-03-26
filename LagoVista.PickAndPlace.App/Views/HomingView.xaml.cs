﻿using LagoVista.PickAndPlace.App.ViewModels;
using System.Windows;
using LagoVista.PickAndPlace.Interfaces;

namespace LagoVista.PickAndPlace.App.Views
{
    public partial class HomingView : Window
    {
        public HomingView(IMachine machine)
        {
            InitializeComponent();
            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                ViewModel = new ViewModels.MVHomingViewModel(machine);

                this.Closing += MachineVision_Closing;
                this.Loaded += MachineVision_Loaded;
            }
        }

        private void MachineVision_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.StopCapture();
        }

        private async void MachineVision_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitAsync();
        }

        public MVHomingViewModel ViewModel
        {
            get { return DataContext as MVHomingViewModel; }
            set { DataContext = value; }
        }

        private void WebCamImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var percentX = (e.GetPosition(WebCamImage).X / WebCamImage.ActualWidth) - 0.5;
            var percentY = -((e.GetPosition(WebCamImage).Y / WebCamImage.ActualHeight) - 0.5);
            var absX = percentX * 38 + ViewModel.Machine.MachinePosition.X;
            var absY = percentY * 28 + ViewModel.Machine.MachinePosition.Y;

            ViewModel.Machine.GotoPoint(absX, absY);
        }

        private void BirdsEye_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var absX = (e.GetPosition(BirdsEye).X) * 2.0;
            var absY = (BirdsEye.ActualHeight - e.GetPosition(BirdsEye).Y) * 2.0;

            ViewModel.Machine.GotoPoint(absX, absY);
        }
    }
}
