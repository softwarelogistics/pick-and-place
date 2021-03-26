using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.ViewModels;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public class MVMachineControlViewModel : MachineVisionViewModelBase
    {
        public MVMachineControlViewModel(IMachine machine) : base(machine)
        {
            MachineControlVM = new MachineControlViewModel(Machine);
        }

        public MachineControlViewModel MachineControlVM { get; private set; }
    }
}
