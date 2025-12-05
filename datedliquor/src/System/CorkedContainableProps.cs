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
    public class CorkedContainableProps
    {
        //Ubject to hold all extra nexxecary attributes
        AssetLocation CorkAddSound = new AssetLocation();

        AssetLocation CorkPopSound = new AssetLocation();
        

        public bool HasDate;

        public double FirstCorkedHours;

        public double LastCorkedHours;

    }
}
