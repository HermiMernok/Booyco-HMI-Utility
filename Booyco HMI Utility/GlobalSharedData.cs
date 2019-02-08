using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    public enum ProgramFlowE
    {
        Startup,
        WiFi,
        USB,
        File,
        Bluetooth,
        GPRS,
        Bootload,
        Confiure,
        Dataview
    }

    public static class ProgramFlow
    {
        public static int ProgramWindow { get; set; }
        public static int SourseWindow { get; set; }
    }

    public static class GlobalSharedData
    {
        public static string ServerStatus { get; set; }
        public static byte[] ServerMessageSend { get; set; }
        public static bool BroadCast { get; set; }
        public static List<NetworkDevice> NetworkDevices = new List<NetworkDevice>();
        public static int SelectedDevice { get; set; }
        public static string WiFiApStatus;

    }
}
