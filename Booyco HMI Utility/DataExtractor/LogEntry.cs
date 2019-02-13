﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    class LogEntry : INotifyPropertyChanged
    {
        private int _number;
        public int Number
        {
            get
            {
                return _number;
            }
            set
            {
                _number = value;
                OnPropertyChanged("Number");
            }
        }

        private DateTime _dateTime;
        public DateTime DateTime
        {
            get
            {
                return _dateTime;
            }
            set
            {
                _dateTime = value;
                OnPropertyChanged("DateTime");
            }
        }

        private UInt16 _eventID;
        public UInt16 EventID
        {
            get
            {
                return _eventID;
            }
            set
            {
                _eventID = value;
                OnPropertyChanged("EventID");
            }
        }

        private string _eventName;
        public string EventName
        {
            get
            {
                return _eventName;
            }
            set
            {
                _eventName = value;
                OnPropertyChanged("EventName");
            }
        }
        private byte[] _rawData;
        public byte[] RawData
        {
            get
            {
                return _rawData;
            }
            set
            {
                _rawData = value;
                OnPropertyChanged("RawData");
            }
        }

        private byte[] _rawEntry;
        public byte[] RawEntry
        {
            get
            {
                return _rawEntry;
            }
            set
            {
                _rawEntry = value;
                OnPropertyChanged("RawEntry");
            }
        }

        private string _eventInfo;
        public string EventInfo
        {
            get
            {
                return _eventInfo;
            }
            set
            {
                _eventInfo = value;
                OnPropertyChanged("EventInfo");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
