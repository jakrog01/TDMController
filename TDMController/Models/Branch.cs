using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text.Json;
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
        internal BranchStates State;
        internal int? BranchIndex { get; private set; }

        internal Branch(SerialPort serialPort, int? branchIndex, RotationDevice? rotationDevice, IPositionDevice? positionDevice)
        {
            SerialPort = serialPort;
            BranchIndex = branchIndex;
            RotationDevice = rotationDevice;
            PositionDevice = positionDevice;
            try
            {
                SerialPort.Open();
                State = BranchStates.Ready;
            }
            catch
            {
                State = BranchStates.Error;
            }
            
            if (SerialPort.IsOpen)
            {
                Task.Run(() => CheckBranchConnection());
            }
        }

        private void CheckBranchConnection()
        {
            if (State == BranchStates.Ready)
            {
                State = BranchStates.Busy;
                SendTriggerCommand("c", 1000);
            }
        }

        public void MoveRotationDevice(int moveValue)
        {
            moveValue %= 360;
            const int TIMEOUT = 10000;
            var value = Convert.ToInt32(Math.Round(moveValue / 360.0 * 2038));

            if (moveValue != 0 && RotationDevice is not null &&
                ((State & BranchStates.Ready) == BranchStates.Ready) &&
                ((RotationDevice.State & RotationDeviceStates.Ready) == RotationDeviceStates.Ready))
            {
                State = BranchStates.Busy;
                try
                {
                    var moveTask = Task.Run(() => RotationDevice.MoveDevice(value));
                    if (!moveTask.Wait(TIMEOUT))
                    {
                        RotationDevice.State = RotationDeviceStates.Error;
                        return;
                    }
                }
                catch
                {
                    State = BranchStates.Error;
                    return;
                }
                RotationDevice.Position += moveValue;
                RotationDevice.Position %= 360;

                if (RotationDevice.Position == 0)
                {
                    RotationDevice.State |= RotationDeviceStates.Home;
                }

                State = BranchStates.Ready;
            }
        }

        public void HomeDevices()
        {
            if (RotationDevice != null && RotationDevice.State == RotationDeviceStates.Ready && State == BranchStates.Ready)
            {
                var MoveTask = Task.Run(() => MoveRotationDevice(-RotationDevice.Position));
                MoveTask.Wait();
            }

            if (PositionDevice != null)
            {
                if (PositionDevice is TLPositionDevice podlDevice)
                {
                    var MoveTask = Task.Run(() => MovePositionDevice(-podlDevice.Position));
                    MoveTask.Wait();
                }

                else if (PositionDevice is TLPositionDevice tlPositionMotor)
                {
                    var MoveTask = Task.Run(() => tlPositionMotor.HomeMotor());
                    MoveTask.Wait();
                }
            }

            Task.Run(() => CheckBranchConnection());
        }
        public void MovePositionDevice(int moveValue)
        {
            if (moveValue == 0 || PositionDevice is null ||
                ((State & BranchStates.Ready) == 0) ||
                ((PositionDevice.State & PositionDeviceStates.Ready) == 0))
            {
                return;
            }

            if (PositionDevice is PODLDevice podl)
            {
                State = BranchStates.Busy;
                int TIMEOUT = 1500 + ((Math.Abs(moveValue) % 360) * 2000);
                var value = Convert.ToInt32(Math.Round(moveValue / 360.0 * 200));
                try
                {
                    var MoveTask = Task.Run(() => podl.MoveDevice(value));
                    if (!MoveTask.Wait(TIMEOUT))
                    {
                        podl.State = PositionDeviceStates.Error;
                        return;
                    }
                }
                catch
                {
                    State = BranchStates.Error;
                    return;
                }
                podl.Position += moveValue;

                if (podl.Position == 0)
                {
                    PositionDevice.State |= PositionDeviceStates.Home;
                }

                State = BranchStates.Ready;
            }

            else if (PositionDevice is TLPositionDevice tlPositionMotor)
            {
                if (tlPositionMotor.KCubeDevice is not null)
                {
                    var MoveTask = Task.Run(() => tlPositionMotor.MoveDevice(moveValue));
                }
            }

        }
        public void MoveMotorsInSequence(List<int> commands)
        {
            int j = 0;
            if (RotationDevice is not null)
            {
                MoveRotationDevice(commands[j]);
                j++;
            }

            if (PositionDevice is not null)
            {
                MovePositionDevice(commands[j]);
            }
            return;
        }

        public void SendExternalDeviceTrigger()
        {
            if (State == BranchStates.Ready)
            {
                State = BranchStates.Busy;
                SendTriggerCommand("t", 2000);
            }
        }

        private void SendTriggerCommand(string commandType, int value)
        {
            int TIMEOUT = 2000 + value;
            string commandJson = JsonSerializer.Serialize(new ArduinoCommand(commandType, value));
            var sendTriggerTask = Task.Run(() => waitForReplay(commandJson));
            bool branchConnection = sendTriggerTask.Wait(TIMEOUT);
            if (!branchConnection)
            {
                State = BranchStates.Error;
                return;
            }

            if (commandType == "t")
            {
                return;
            }

            if (RotationDevice is not null)
            {
                RotationDevice.State &= ~RotationDeviceStates.Error;
                RotationDevice.State |= RotationDeviceStates.Ready;
            }

            if (PositionDevice is not null)
            {
                PositionDevice.State &= ~PositionDeviceStates.Error;
                PositionDevice.State |= PositionDeviceStates.Ready;
            }

            State = BranchStates.Ready;

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
                    State = BranchStates.Error;
                }
            }
        }
    }
}
