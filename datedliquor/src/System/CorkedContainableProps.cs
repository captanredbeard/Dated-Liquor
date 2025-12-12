using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace datedliquor.src.System
{
    public class DatedContainableProps
    {
        //Whether it has a date attached to the container
        public bool HasDate;

        //Which player bottled it.
        public string bottledBy;

        public double FirstCorkedHours;

        public double LastCorkedHours;

        //Conditions for when to remove the dates. (should likely put these into the global config)
        public bool RemoveDateOnUncork;

        public bool RemoveDateOnAddFluid;

        public bool RemoveDateOnEmpty;
    }
}
