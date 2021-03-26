using LagoVista.PickAndPlace.ViewModels;
using System.Windows.Controls;

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for ManualSend.xaml
    /// </summary>
    public partial class ManualSend : UserControl
    {
        public ManualSend()
        {
            InitializeComponent();
        }
    
        public GCodeAppViewModelBase ViewModel
        {
            get { return DataContext as GCodeAppViewModelBase; }
        }

        private void TextBoxManual_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Up)
            {
                ViewModel.ShowPrevious();
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                ViewModel.ShowNext();
            }
            else if (e.Key == System.Windows.Input.Key.Return)
            {
                e.Handled = true;
                ViewModel.ManualSend();
            }
        }

        private void TextBoxManual_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TextBoxManual.CaretIndex == 0)
            {
                TextBoxManual.CaretIndex = TextBoxManual.Text.Length;
            }
        }
    }
}
