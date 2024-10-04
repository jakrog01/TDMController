using System.IO.Ports;

namespace TDMController.Models.TDMDevices
{
    internal interface ITDMDevice
    {
        public int Position { get; set; }
        public abstract string ToJson();

    }
}
