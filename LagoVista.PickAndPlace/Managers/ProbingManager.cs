using LagoVista.Core.PlatformSupport;
using LagoVista.PickAndPlace.Interfaces;

namespace LagoVista.PickAndPlace.Managers
{
    public partial class ProbingManager : ManagerBase, IProbingManager
    {
        public ProbingManager(IMachine machine, ILogger logger) : base(machine, logger)
        {

        }
    }
}
