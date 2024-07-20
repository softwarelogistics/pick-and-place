using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    public class PnPMachine
    {   
        public PnPMachine()
        {
            PartStrips = new ObservableCollection<PartStrip>();
            Packages = new ObservableCollection<Package>();
            StripFeederPackages = new ObservableCollection<StripFeederPackage>();
        }

        public void SortPartStrips()
        {
            var orderedStrips = PartStrips.OrderByDescending(str => str.ReferenceHoleY).ToList();
            PartStrips.Clear();
            foreach(var strip in orderedStrips)
            {
                PartStrips.Add(strip);
            }
        }

        public ObservableCollection<PartStrip> PartStrips{ get; set; }

        public ObservableCollection<Package> Packages { get; set; }    

        public ObservableCollection<StripFeederPackage> StripFeederPackages { get; set; }
    }
}
