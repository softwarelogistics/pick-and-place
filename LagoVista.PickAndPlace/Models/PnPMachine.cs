using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Models
{
    public class PnPMachine
    {   
        public PnPMachine()
        {
            PartStrips = new ObservableCollection<PartStrip>();
            Packages = new ObservableCollection<Package>();
        }

        public ObservableCollection<PartStrip> PartStrips{ get; set; }

        public ObservableCollection<Package> Packages { get; set; }    
    }
}
