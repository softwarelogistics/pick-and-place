using LagoVista.Core.PlatformSupport;
using LagoVista.PickAndPlace.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Managers
{
    public partial class ToolChangeManager : ManagerBase, IToolChangeManager
    {
        public ToolChangeManager(IMachine machine, ILogger logger) : base(machine, logger)
        {

        }
    }
}
