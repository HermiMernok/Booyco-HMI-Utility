using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

    public class ParameterEnum
    {
        public int enumVal;
        public string enumName;
    }
}
