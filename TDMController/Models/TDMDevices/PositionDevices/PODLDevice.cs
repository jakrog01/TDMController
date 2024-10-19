using HarfBuzzSharp;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.Json;
using TDMController.Models.TDMDevices.States;

namespace TDMController.Models.TDMDevices
{
    internal class PODLDevice: IPositionDevice
    {
        public PositionDeviceStates State { get; set; }
        public int Position { get; set; }
        private SerialPort? _serialPort;

        internal PODLDevice(SerialPort serialPort)
        {
            _serialPort = serialPort;
            Position = 0;
        }

        public void MoveDevice(int value)
        {
            State = PositionDeviceStates.Busy;
            string commandJson = JsonSerializer.Serialize(new ArduinoCommand("p", value));
            _serialPort!.Write(commandJson + "\n");
            while (true)
            {
                string receivedData = _serialPort.ReadLine();
                if (receivedData.Contains("Done"))
                {
                    State = PositionDeviceStates.Ready;
                    break;
                }
                else if (receivedData.Contains("LimitPlus"))
                {
                    State = PositionDeviceStates.Ready;
                    State |= PositionDeviceStates.Limit;
                    break;
                }
                else if (receivedData.Contains("LimitMinus"))
                {
                    State = PositionDeviceStates.Ready;
                    State |= PositionDeviceStates.Home;
                    break;
                }
            }
        }
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
