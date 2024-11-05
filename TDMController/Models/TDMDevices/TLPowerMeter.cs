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
        TLPM? PowerMeterDevice { get; set; }

        public TLPowerMeter(string pmID)
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
                double refPower;
                int err = PowerMeterDevice.measPower(out powerValue);
                int err2 = PowerMeterDevice.getPowerRef(2, out refPower);
                powerValue -= refPower;
                return $"{FormatWithMetricPrefix(powerValue).ToString()}W";
            }

            return "ND";
        }

        public static string FormatWithMetricPrefix(double value)
        {
            double absValue = Math.Abs(value);

            if (absValue >= 1e9)
                return $"{value / 1e9:F2} G";
            else if (absValue >= 1e6)
                return $"{value / 1e6:F2} M";
            else if (absValue >= 1e3)
                return $"{value / 1e3:F2} k";
            else if (absValue >= 1e2)
                return $"{value / 1e2:F2} h";
            else if (absValue >= 1e1)
                return $"{value / 1e1:F2} da";
            else if (absValue >= 1)
                return $"{value:F2} ";
            else if (absValue >= 1e-1)
                return $"{value * 1e1:F2} d";
            else if (absValue >= 1e-2)
                return $"{value * 1e2:F2} c";
            else if (absValue >= 1e-3)
                return $"{value * 1e3:F2} m";
            else if (absValue >= 1e-6)
                return $"{value * 1e6:F2} µ";
            else if (absValue >= 1e-9)
                return $"{value * 1e9:F2} n";
            else if (absValue >= 1e-12)
                return $"{value * 1e12:F2} p";
            else
                return $"{value}";
        }
    }
}
