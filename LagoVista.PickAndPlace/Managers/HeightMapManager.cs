using LagoVista.PickAndPlace.Interfaces;
using LagoVista.Core.PlatformSupport;

namespace LagoVista.PickAndPlace.Managers
{
    public partial class HeightMapManager : ManagerBase, IHeightMapManager
    {
        IPCBManager _boardManager;

        public HeightMapManager(IMachine machine, ILogger logger, IPCBManager boardManager) : base (machine, logger)
        {
            _boardManager = boardManager;
        }
    }
}
