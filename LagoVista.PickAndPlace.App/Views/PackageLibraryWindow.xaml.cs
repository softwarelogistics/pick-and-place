using LagoVista.PickAndPlace.ViewModels;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Views
{
    /// <summary>
    /// Interaction logic for PackageLibraryWindow.xaml
    /// </summary>
    public partial class PackageLibraryWindow : Window
    {
        public PackageLibraryWindow()
        {
            InitializeComponent();
            DataContext = new PackageLibraryViewModel();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
