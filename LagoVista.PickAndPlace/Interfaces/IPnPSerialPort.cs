using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IPnPSerialPort : ISerialPort
    {
        Stream InputStream { get; }
        Stream OutputStream { get; }
    }
}
