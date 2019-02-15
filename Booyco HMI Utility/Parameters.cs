using System;
using System.Collections.Generic;
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
        public int Ptype;
        public int enumVal;
        public List<string> parameterEnums;

    }
    
    public class ParametersDisplay
    {
        public int OriginIndx { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
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
