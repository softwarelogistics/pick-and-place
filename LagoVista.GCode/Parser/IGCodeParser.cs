using LagoVista.GCode.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.GCode.Parser
{
    public interface IGCodeParser
    {
        GCodeCommand ParseLine(string line, int index);
    }
}
