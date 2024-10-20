using System;
using System.IO.Ports;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TDMController.Models.TDMDevices;
using TDMController.Models.TDMDevices.PositionDevices;
using TDMController.Models.TDMDevices.States;

namespace TDMController.Models
{
    public class Branch
    {
        internal SerialPort SerialPort { get;}
        internal RotationDevice? RotationDevice { get; }
        internal IPositionDevice? PositionDevice { get; }
        internal BranchStates BranchState;
        internal int? BranchIndex { get; private set; }

        internal Branch(SerialPort serialPort, int? branchIndex, RotationDevice? rotationDevice, IPositionDevice? positionDevice)
        {
            SerialPort = serialPort;
            BranchState = BranchStates.Connection;
            BranchIndex = branchIndex;
            RotationDevice = rotationDevice;
            PositionDevice = positionDevice;
            try
            {
                SerialPort.Open();
                BranchState = BranchStates.Connection;
            }
            catch
            {
                BranchState = BranchStates.Error;
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

            if (moveValue != 0 && RotationDevice is not null)
            {
                try
                {
                    var moveTask = Task.Run(() => RotationDevice.MoveDevice(value));
                    if (!moveTask.Wait(TIMEOUT))
                    {
                        RotationDevice.State = RotationDeviceStates.Error;
                    }
                }
                catch
                {
                    BranchState = BranchStates.Error;
                }
                RotationDevice.Position += moveValue;
                RotationDevice.Position %= 360;
            }
        }

        public void MovePositionDevice(int moveValue)
        {
            if (moveValue == 0 || PositionDevice is null)
            {
                return;
            }

            if (PositionDevice is PODLDevice podl)
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
                    BranchState = BranchStates.Error;
                }
            }

            else if (PositionDevice is TLPositionDevice tlPositionMotor)
            {
                if (tlPositionMotor.KCubeDevice is not null)
                {
                    var MoveTask = Task.Run(() => tlPositionMotor.MoveDevice(moveValue));
                }
            }


        }

        public void SendExternalDeviceTrigger()
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
                BranchState = BranchStates.Error;
            }

            void waitForReplay(string commandJson)
            {
                try
                {
                    SerialPort.Write(commandJson + "\n");
                    while (true)
                    {
                        string receivedData = SerialPort.ReadLine();
                        if (receivedData.Contains("Done"))
                        {
                            break;
                        }
                    }
                }
                catch
                {
                    BranchState = BranchStates.Error;
                }
            }
        }
    }
}
