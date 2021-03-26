using System;

namespace LagoVista.PickAndPlace
{
    public partial class Machine
    {
        public void SendCommand(String cmd)
        {
            if (AssertConnected())
            {
                Enqueue(cmd);
            }
        }
    }
}
