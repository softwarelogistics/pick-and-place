﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface ISocketClient : IDisposable
    {
        Task ConnectAsync(String ipAddress, int port);
        Stream InputStream { get; }
        Stream OutputStream { get; }
        Task CloseAsync();

    }
}
