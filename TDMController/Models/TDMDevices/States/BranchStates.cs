using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDMController.Models.TDMDevices.States
{
    [Flags]
    internal enum BranchStates
    {
        Ready = 1,
        Busy = 2,
        Error = 4
    }
}
