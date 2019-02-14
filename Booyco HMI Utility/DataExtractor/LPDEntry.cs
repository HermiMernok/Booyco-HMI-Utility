using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    class LPDEntry : INotifyPropertyChanged
    {
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

        private List<LPDDataLookupEntry> _data;
        public List<LPDDataLookupEntry> Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                OnPropertyChanged("Data");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    class LPDDataLookupEntry : INotifyPropertyChanged
    {
        private string _dataLink;
        public string DataLink
        {
            get
            {
                return _dataLink;
            }
            set
            {
                _dataLink = value;
                OnPropertyChanged("DataLink");
            }
        }

        private string _dataName;
        public string DataName
        {
            get
            {
                return _dataName;
            }
            set
            {
                _dataName = value;
                OnPropertyChanged("DataName");

            }
        }
        private int _numberBytes;
        public int NumberBytes
        {
            get
            {
                return _numberBytes;
            }
            set
            {
                _numberBytes = value;
                OnPropertyChanged("NumberBytes");
            }
        }

        private int _scale;
        public int Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
                OnPropertyChanged("Scale");
            }
        }

        private bool _isInt;
        public bool IsInt
        {
            get
            {
                return _isInt;
            }
            set
            {
                _isInt = value;
                OnPropertyChanged("IsInt");
            }
        }

        private string _appendix;
        public string Appendix
        {
            get
            {
                return _appendix;
            }
            set
            {
                _appendix = value;
                OnPropertyChanged("Appendix");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
