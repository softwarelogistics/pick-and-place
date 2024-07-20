using LagoVista.PCB.Eagle.Models;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Models
{
    public class PlaceableParts
    {
        public int Count { get; set; }

        public ObservableCollection<Component> Parts {get; set;}
        
        public string Package { get; set; }
        public string Value { get; set; }

        public PartStrip PartStrip { get; set; }

        public string StripFeederPackage { get; set; }
        public string StripFeeder { get; set; }
    }
}
