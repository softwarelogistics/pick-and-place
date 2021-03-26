using LagoVista.PickAndPlace.Interfaces;
using System.Windows;

namespace LagoVista.PickAndPlace.App
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow(IMachine machine)
        {
            InitializeComponent();
            DataContext = machine;
        }
    }
}
