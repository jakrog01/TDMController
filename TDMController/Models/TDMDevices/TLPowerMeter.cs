using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thorlabs.TLPM_64.Interop;

namespace TDMController.Models.TDMDevices
{
    internal class TLPowerMeter
    {
        TLPM PowerMeterDevice { get; set; }

        public TLPowerMeter(string pmID)
        {
            Task.Run (() => Initialize(pmID));        
        }

        public void Initialize(string pmID)
        {
            try
            {
                var Instrument_Handle = new HandleRef();
                var searchDevice = new TLPM(Instrument_Handle.Handle);
                uint count = 0;
                string firstPowermeterFound = "";

                try
                {
                    int pInvokeResult = searchDevice.findRsrc(out count);

                    if (count > 0)
                    {
                        StringBuilder descr = new StringBuilder(1024);

                        searchDevice.getRsrcName(0, descr);
                        firstPowermeterFound = descr.ToString();
                    }
                }
                catch { }

                if (count == 0)
                {
                    searchDevice.Dispose();
                    return;
                }

                TLPM pm2 = new TLPM(pmID, true, false);
                double powerValue;
                PowerMeterDevice = pm2;
                int err = PowerMeterDevice.measPower(out powerValue);
            }
            catch (Exception e)
            {
                return;
            }
        }

        public string MeasurePower()
        {
            if (PowerMeterDevice is not null)
            {
                double powerValue;
                int err = PowerMeterDevice.measPower(out powerValue);
                return powerValue.ToString();
            }

            return "No device detected";
        }

        public string MeasurePowerWithUnit()
        {
            if (PowerMeterDevice is not null)
            {
                double powerValue;
                int err = PowerMeterDevice.measPower(out powerValue);
                return $"{FormatWithMetricPrefix(powerValue).ToString()}W";
            }

            return "ND";
        }

        public static string FormatWithMetricPrefix(double value)
        {
            double absValue = Math.Abs(value);

            if (absValue >= 1e-3)
                return $"{value:F3} ";
            else if (absValue >= 1e-6)
                return $"{value * 1e6:F3} µ";
            else if (absValue >= 1e-9)
                return $"{value * 1e9:F3} n";
            else if (absValue >= 1e-12)
                return $"{value * 1e12:F3} p";
            else
                return $"{value}";
        }
    }
}
