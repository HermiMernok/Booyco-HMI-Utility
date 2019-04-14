using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility.CustomMarker
{

    public enum MarkerType
    {
        Indicator,
        Ellipse,
        Point,
        Cross
    }

    public class MarkerEntry
    {

        public GMapMarker MapMarker;
        public string title;
        public uint Zone;
        public double Heading;
        public uint Type;
        public ushort Width;
        public double Scale;
        public ushort Height;
        public int PresenceZoneSize;
        public int WarningZoneSize;
        public int CriticalZoneSize;
        public double Latitude;
        public double Longitude;
        public double Accuracy;
        public double BrakeDistance;
    }
}
