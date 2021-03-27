using LagoVista.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LagoVista.PickAndPlace.App.Services
{
    public class NuvIoTDispatcher : IDispatcherServices
    {
        Dispatcher _dispatcher;
        public NuvIoTDispatcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Invoke(Action action)
        {
            _dispatcher.BeginInvoke(action);
        }
    }
}
