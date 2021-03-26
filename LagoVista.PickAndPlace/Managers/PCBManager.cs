using LagoVista.Core.PlatformSupport;
using LagoVista.PickAndPlace.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Managers
{
    public partial class PCBManager : Core.Models.ModelBase, IPCBManager
    {
        IMachine _machine;
        ILogger _logger;

        public PCBManager(IMachine machine, ILogger logger)
        {
            _logger = logger;
            _machine = machine;
        }

        public IMachine Machine
        {
            get { return _machine; }
        }
    }
}
