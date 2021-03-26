﻿using LagoVista.PickAndPlace.App.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Views
{
    /// <summary>
    /// Interaction logic for WorkAlignmentView.xaml
    /// </summary>
    public partial class WorkAlignmentView : Window
    {
        public WorkAlignmentView(IMachine machine)
        {
            InitializeComponent();

            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                ViewModel = new WorkAlignmentViewModel(machine);

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

        public WorkAlignmentViewModel ViewModel
        {
            get { return DataContext as WorkAlignmentViewModel; }
            set{DataContext = value;}
        }
    }
}
