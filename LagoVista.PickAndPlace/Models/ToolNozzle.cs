using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Models
{
    public class ToolNozzle
    {
        public ToolNozzle()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public string Name { get; set; }
        public double SafeMoveHeight { get; set; }
        public double PickHeight { get; set; }
        public double BoardHeight { get; set; }
    }
}
