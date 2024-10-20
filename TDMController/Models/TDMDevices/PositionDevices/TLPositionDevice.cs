using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.KCube.BrushlessMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.KCubeMotor;
using TDMController.Models.TDMDevices.States;
using System.Threading;
using System.Text.Json;

namespace TDMController.Models.TDMDevices.PositionDevices
{
    internal class TLPositionDevice : IPositionDevice
    {
        public PositionDeviceStates State { get; set; }
        public string SerialNumber { get; set; }
        public int Position { get; set; }

        public KCubeBrushlessMotor KCubeDevice { get; private set; }

        public TLPositionDevice(string serialNumber)
        {
            SerialNumber = serialNumber;
            Position = 0;
            var initTask = Task.Run(() => InitializeTLPositionMotor());
        }

        public void InitializeTLPositionMotor()
        {
            DeviceManagerCLI.BuildDeviceList();
            List<string> devices = DeviceManagerCLI.GetDeviceList();

            if (!devices.Contains(SerialNumber))
            {
                State = PositionDeviceStates.Error;
                return;
            }

            KCubeDevice = KCubeBrushlessMotor.CreateKCubeBrushlessMotor(SerialNumber);

            if (KCubeDevice == null)
            {
                State = PositionDeviceStates.Error;
                return;
            }

            KCubeDevice.Connect(SerialNumber);

            if (!KCubeDevice.IsSettingsInitialized())
            {
                try
                {
                    KCubeDevice.WaitForSettingsInitialized(5000);
                }
                catch (Exception)
                {
                    State = PositionDeviceStates.Error;
                    return;
                }
            }

            if (KCubeDevice is not null)
            {
                try
                {
                    KCubeDevice.StartPolling(250);
                    Thread.Sleep(500);
                    KCubeDevice.EnableDevice();
                    Thread.Sleep(500);

                    var motorConfiguration = KCubeDevice.LoadMotorConfiguration(SerialNumber);
                    KCubeBrushlessMotorSettings currentDeviceSettings = KCubeDevice.MotorDeviceSettings as KCubeBrushlessMotorSettings;
                    DeviceInfo deviceInfo = KCubeDevice.GetDeviceInfo();

                    KCubeDevice.Home(60000);
                }
                catch
                {
                    State = PositionDeviceStates.Error;
                    return;
                }

                State = PositionDeviceStates.Ready;
            }
        }

        public void MoveDevice(int value)
        {
            try
            {
                if (KCubeDevice is not null)
                {
                    double v = 0.299792458;
                    int movement = Convert.ToInt32(Math.Round(value * v));
                    int max = Convert.ToInt32(Math.Round(320 * v));
                    int afterMovement = Position + movement;

                    if (afterMovement > 0 && afterMovement < max)
                    {
                        KCubeDevice.MoveTo(afterMovement, 60000);
                        Position = afterMovement;
                    }

                    else if (afterMovement <= 0)
                    {
                        KCubeDevice.Home(60000);
                        Position = 0;
                    }

                    else if (afterMovement >= max)
                    {
                        KCubeDevice.MoveTo(max, 60000);
                        Position = max;
                    }
                }
            }
            catch
            {
                State = PositionDeviceStates.Error;
            }
        }
        public string ToJson() 
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
