using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Booyco_HMI_Utility
{
    public class Parameters
    {
        public string Name;
        public int CurrentValue;
        public int MaximumValue;
        public int MinimumValue;
        public int DefaultValue;
        public string Group;
        public string SubGroup;
        public int Ptype;
        public int enumVal;
        public List<string> parameterEnums;

    }
    
    public class ParametersDisplay : INotifyPropertyChanged
    {
        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion

        public int OriginIndx { get; set; }
        public string Group { get; set; }
        public string SubGroup { get; set; }
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; OnPropertyChanged("Name"); }
        }
        private string _value;
        public string Value
        {
            get { return _value; }
            set { _value = value; OnPropertyChanged("Value"); }
        }
        public Visibility BtnVisibility { get; set; }
        public Visibility textBoxVisibility { get; set; }
        public Visibility dropDownVisibility { get; set; }
        public bool LablEdit { get; set; }
        public List<string> parameterEnums { get; set; }
        public int EnumIndx { get; set; }
    }

    public class ParameterEnum
    {
        public int enumVal;
        public string enumName;
    }
}
