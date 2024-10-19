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
        internal SerialPort _serialPort { get;}
        internal RotationDevice? _rotationDevice { get; }
        internal IPositionDevice? _positionDevice { get; }
        internal BranchStates _branchState;
        internal int? BranchIndex { get; private set; }

        internal Branch(SerialPort serialPort, int? branchIndex, RotationDevice? rotationDevice, IPositionDevice? positionDevice)
        {
            _serialPort = serialPort;
            _branchState = BranchStates.Connection;
            BranchIndex = branchIndex;
            _rotationDevice = rotationDevice;
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

        public void MoveRotationDevice(int moveValue)
        {
            moveValue %= 360;
            const int TIMEOUT = 10000;
            var value = Convert.ToInt32(Math.Round(moveValue / 360.0 * 2038));

            if (moveValue != 0 && _rotationDevice is not null)
            {
                try
                {
                    var moveTask = Task.Run(() => _rotationDevice.MoveDevice(value));
                    if (!moveTask.Wait(TIMEOUT))
                    {
                        _rotationDevice.State = RotationDeviceStates.Error;
                    }
                }
                catch
                {
                    _branchState = BranchStates.Error;
                }
                _rotationDevice.Position += moveValue;
                _rotationDevice.Position %= 360;
            }
        }

        public void MovePositionDevice(int moveValue)
        {
            if (moveValue == 0 || _positionDevice is null)
            {
                return;
            }

            if (_positionDevice is PODLDevice podl)
            {
                int TIMEOUT = 1500 + ((Math.Abs(moveValue) % 360) * 2000);
                var value = Convert.ToInt32(Math.Round(moveValue / 360.0 * 200));
                try
                {
                    var MoveTask = Task.Run(() => podl.MoveDevice(value));
                    if (!MoveTask.Wait(TIMEOUT))
                    {
                        podl.State = PositionDeviceStates.Error;
                    }
                    podl.Position += moveValue;
                }
                catch
                {
                    _branchState = BranchStates.Error;
                }
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
