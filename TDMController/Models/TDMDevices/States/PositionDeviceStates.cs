using System;
namespace TDMController.Models.TDMDevices.States
{
    [Flags]
    internal enum PositionDeviceStates
    {
        Error = 1,
        Ready = 2,
        Busy = 4,
        Home = 8,
        Limit = 16
    }
}
