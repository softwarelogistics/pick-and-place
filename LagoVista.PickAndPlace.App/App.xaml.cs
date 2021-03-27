using LagoVista.Core;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.PickAndPlace.App.Services;
using LagoVista.PickAndPlace.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LagoVista.PickAndPlace.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SLWIOC.Register<IDispatcherServices>(new NuvIoTDispatcher(Dispatcher));
            SLWIOC.Register<ISocketClient, SocketClient>();
            SLWIOC.RegisterSingleton<ILogger, DebugLogger>();
            SLWIOC.RegisterSingleton<IDeviceManager, DeviceManager>();
            SLWIOC.RegisterSingleton<IPopupServices, PopupService>();
            SLWIOC.RegisterSingleton<IStorageService, StorageService>();
        }
    }
}
