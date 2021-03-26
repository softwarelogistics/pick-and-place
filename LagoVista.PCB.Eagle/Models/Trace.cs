using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PCB.Eagle.Models
{
    public class Trace
    {
        public Trace()
        {
            Wires = new List<Wire>();
        }

        public List<Wire> Wires { get; set; }
    }
}
