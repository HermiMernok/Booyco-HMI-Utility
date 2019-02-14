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
        public int type;
        public int enumVal;
        public List<ParameterEnum> parameterEnums;
    }    

    public class ParameterEnum
    {
        public int enumVal;
        public string enumName;
    }
}
