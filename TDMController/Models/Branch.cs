using System;
using System.IO.Ports;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TDMController.Models.TDMDevices;
using TDMController.Models.TDMDevices.States;

namespace TDMController.Models
{
    public class Branch
    {
        private SerialPort _serialPort;
        private RotationDevice? _rotationDevice;
        private IPositionDevice? _positionDevice;
        internal BranchStates _branchState;
        internal int BranchIndex { get; private set; }

        internal Branch(string com, int baudRate, int branchIndex, RotationDevice? rotationDevice, IPositionDevice? positionDevice)
        {
            _serialPort = new SerialPort(com, baudRate);
            _branchState = BranchStates.Connection;
            BranchIndex = branchIndex;
            _serialPort = new SerialPort(com, baudRate);
            _rotationDevice = rotationDevice;
            if (_rotationDevice is not null)
            {
                _rotationDevice.SetSerialPort(_serialPort);
            }
            _positionDevice = positionDevice;
            try
            {
                _serialPort.Open();
                _branchState = BranchStates.Connection;
            }
            catch
            {
                _branchState = BranchStates.Error;
            }
            Task.Run(() => CheckBranchConnection());
        }

        private void CheckBranchConnection()
        {
            SendTriggerCommand("c", 2000);
        }

        public void MoveRotationDevice(int angle)
        {
            angle %= 360;
            int TIMEOUT = 10000;
            int value = Convert.ToInt32(Math.Round(angle / 360.0 * 2038));

            if (angle !=0 && _rotationDevice is not null)
            {
                try
                {
                    var MoveTask = Task.Run(() => _rotationDevice.MoveDevice(value));
                    bool branchConnection = MoveTask.Wait(TIMEOUT);
                    if (!branchConnection)
                    {
                        _rotationDevice.State = RotationDeviceStates.Error;
                    }
                }
                catch
                {
                    _branchState = BranchStates.Error;
                }
                _rotationDevice.Position += angle;
                _rotationDevice.Position %= 360;
            }
        }

        private void SendExternalDeviceTrigger()
        {
            SendTriggerCommand("t", 2000);
        }

        private void SendTriggerCommand(string commandType, int value)
        {
            int TIMEOUT = 2000 + value;
            string commandJson = JsonSerializer.Serialize(new ArduinoCommand(commandType, value));
            var sendTriggerTask = Task.Run(() => waitForReplay(commandJson));
            bool branchConnection = sendTriggerTask.Wait(TIMEOUT);
            if (!branchConnection)
            {
                _branchState = BranchStates.Error;
            }

            void waitForReplay(string commandJson)
            {
                try
                {
                    _serialPort.Write(commandJson + "\n");
                    while (true)
                    {
                        string receivedData = _serialPort.ReadLine();
                        if (receivedData.Contains("Done"))
                        {
                            break;
                        }
                    }
                }
                catch
                {
                    _branchState = BranchStates.Error;
                }
            }
        }
    }
}
