using LagoVista.PickAndPlace.Models;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Managers
{
    public class PnPMachineManager
    {
        public static void ResolvePart(PnPMachine machine, PlaceableParts part)
        {
            // todo for common parts, we may have more then one strip, if so pick one with most parts.
            var strip = machine.PartStrips.Where(str => str.PackageName == part.Package && str.Value == part.Value && str.Ready).SingleOrDefault();
            part.PartStrip = strip;
        }

        public static Task<PnPMachine> GetPnPMachineAsync(string path)
        {
            return Core.PlatformSupport.Services.Storage.GetAsync<PnPMachine>(path);
        }

        public static Task SavePackagesAsync(PnPMachine machine, string path)
        {
            return Core.PlatformSupport.Services.Storage.StoreAsync(machine, path);
        }
    }
}
