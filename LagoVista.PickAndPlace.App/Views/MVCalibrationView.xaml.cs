using LagoVista.PickAndPlace.App.ViewModels;
using System.Windows;
using LagoVista.PickAndPlace.Interfaces;

namespace LagoVista.PickAndPlace.App.Views
{
    /// <summary>
    /// Interaction logic for MVCalibrationView.xaml
    /// </summary>
    public partial class MVCalibrationView : Window
    {
        public MVCalibrationView(IMachine machine)
        {
            InitializeComponent();
            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                ViewModel = new CalibrationViewModel(machine);

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

        public CalibrationViewModel ViewModel
        {
            get { return DataContext as CalibrationViewModel; }
            set
            {
                DataContext = value;
            }
        }
    }
}
