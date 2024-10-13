using System.IO.Ports;
using System.Text.Json;
using TDMController.Models.TDMDevices.States;


namespace TDMController.Models.TDMDevices
{
    internal class RotationDevice : ITDMDevice
    {
        internal RotationDeviceStates State { get; set; }
        public int Position { get; set; }
        private readonly int Direction;
        private SerialPort? _serialPort;

        internal RotationDevice(int direction)
        {
            Direction = direction;
            Position = 0;
        }

        public void SetSerialPort(SerialPort serialPort)
        {
            _serialPort = serialPort;
            if (serialPort.IsOpen)
            {
                State = RotationDeviceStates.Ready;
            }
            else
            {
                State = RotationDeviceStates.Error;
            }
        }

        public void MoveDevice(int value)
        {
            State = RotationDeviceStates.Busy;
            string commandJson = JsonSerializer.Serialize(new ArduinoCommand("r", value * Direction));
            _serialPort.Write(commandJson + "\n");
            while (true)
            {
                string receivedData = _serialPort.ReadLine();
                if (receivedData.Contains("Done"))
                {
                    State = RotationDeviceStates.Ready;
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
