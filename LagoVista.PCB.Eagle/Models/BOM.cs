using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PCB.Eagle.Models
{
    public class BOM
    {
        PrintedCircuitBoard _board;

        public BOM(PrintedCircuitBoard board)
        {
            _board = board;
            Entries = new List<BOMEntry>();
            CreateEntries();
        }

        private void CreateEntries()
        {
            foreach(var component in _board.Components)
            {
                var entry = Entries.Where(entr =>
                            entr.Package.Name == component.Package.Name &&
                            entr.Value == component.Value).FirstOrDefault();
                if(entry == null)
                {
                    entry = new BOMEntry()
                    {
                        Package = component.Package,
                        Value = component.Value
                    };
                    Entries.Add(entry);
                }

                entry.Components.Add(component);
            }
        }

        public List<BOMEntry> Entries { get; private set; }
        public List<BOMEntry> SMDEntries { get { return Entries.Where(prt => prt.Package.IsSMD).ToList(); } }

    }
}
