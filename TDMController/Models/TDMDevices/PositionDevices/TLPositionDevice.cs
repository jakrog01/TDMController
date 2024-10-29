using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TDMController.Models.TDMDevices.States;
using System.Threading;
using System.Text.Json;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.KCube.BrushlessMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.KCubeMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using Thorlabs.MotionControl.PrivateInternal.Settings;
using Thorlabs.MotionControl.DeviceManagerCLI;



namespace TDMController.Models.TDMDevices.PositionDevices
{
    internal class TLPositionDevice : IPositionDevice
    {
        public PositionDeviceStates State { get; set; }
        public string SerialNumber { get; set; }
        public int Position { get; set; }

        public KCubeBrushlessMotor? KCubeDevice { get; private set; } = null;

        public TLPositionDevice(string serialNumber)
        {
            SerialNumber = serialNumber;
            Position = 0;
            Task.Run(InitializeTLPositionDevice);
        }

        public void InitializeTLPositionDevice()
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

        public void HomeMotor()
        {
            try
            {
                if (KCubeDevice is not null && ((State & PositionDeviceStates.Error) != PositionDeviceStates.Error))
                {
                    KCubeDevice.Home(60000);
                    Position = 0;
                }

                if (KCubeDevice is not null)
                {
                    KCubeDevice.DisableDevice();
                    Thread.Sleep(1000);
                    KCubeDevice.EnableDevice();
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
