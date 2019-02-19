﻿using Booyco_HMI_Utility.CustomMarker;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        Dataview,
        Mapview
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
        public static bool  ViewMode = false;
        public static string FilePath = "";
        public static List<MarkerEntry> PDSMapMarkers = new List<MarkerEntry>();

    }

    public class GeneralFunctions
    {
        #region General Functions
        public string StringConditioner(string value)
        {
            string string2 = value;
            string[] name2;
            Regex regexItem = new Regex(@"[^A-Z0-9 _]");
            if (string2.Length <= 15)
            {
                string2 = string2.ToUpper();
            }
            else
            {
                string2 = string2.Substring(0, 15).ToUpper();
                //               MessageBox.Show("Tag name my not exceed a length of 15");
            }


            if (!regexItem.IsMatch(string2))
            {
                value = string2;
            }
            else
            {
                //                MessageBox.Show("Tag name my not not contain any special charcters");
                name2 = regexItem.Split(string2);
                string2 = "";
                for (int i = 0; i < name2.Length; i++)
                {
                    if (name2[i] != "")
                    {
                        string2 = string2 + name2[i];
                    }

                }
                //name = regexItem.Replace(name, "$");
                value = string2;
            }

            return value;
        }
        public string StringNumConditioner(string value)
        {
            string string2 = value;
            string[] name2;
            Regex regexItem = new Regex(@"[^0-9]");
            if (string2.Length <= 15)
            {
                string2 = string2.ToUpper();
            }
            else
            {
                string2 = string2.Substring(0, 15).ToUpper();
                //               MessageBox.Show("Tag name my not exceed a length of 15");
            }


            if (!regexItem.IsMatch(string2))
            {
                value = string2;
            }
            else
            {
                //                MessageBox.Show("Tag name my not not contain any special charcters");
                name2 = regexItem.Split(string2);
                string2 = "";
                for (int i = 0; i < name2.Length; i++)
                {
                    if (name2[i] != "")
                    {
                        string2 = string2 + name2[i];
                    }

                }
                //name = regexItem.Replace(name, "$");
                value = string2;
            }

            ulong test;

            if (value != "")
            {
                test = Convert.ToUInt64(value);
                if (test <= 0)
                    test = 1;
                else if (test > ushort.MaxValue)
                    test = ushort.MaxValue;

                value = test.ToString();
            }



            return value;
        }

        #endregion
    }
}
