using LagoVista.PickAndPlace.Interfaces;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class JobControlViewModel : GCodeAppViewModelBase
    {
        public JobControlViewModel(IMachine machine) : base(machine)
        {
            InitCommands();
        }
    }
}
