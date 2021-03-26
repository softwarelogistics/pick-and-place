using LagoVista.PickAndPlace.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class NewHeightMapViewModel : GCodeAppViewModelBase
    {
      
        public NewHeightMapViewModel(IMachine machine) : base(machine)
        {
            InitCommands();
        }
    }
}
