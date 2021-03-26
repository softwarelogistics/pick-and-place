

using LagoVista.PickAndPlace.Interfaces;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class MachineControlViewModel : GCodeAppViewModelBase
    {
        public MachineControlViewModel(IMachine machine) : base(machine)
        {
            InitCommands();
            if(machine.Settings != null)
            {
                XYStepMode = Machine.Settings.XYStepMode;
                ZStepMode = Machine.Settings.ZStepMode;
            }
        }      
    }
}
