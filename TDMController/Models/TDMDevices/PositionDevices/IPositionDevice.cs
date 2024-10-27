using TDMController.Models.TDMDevices;
using TDMController.Models.TDMDevices.States;

namespace TDMController.Models.TDMDevices
{
    internal interface IPositionDevice : ITDMDevice
    {
        internal PositionDeviceStates State { get; set; }
    }
}
