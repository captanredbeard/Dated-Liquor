using ACulinaryArtillery;
using datedliquor.src.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using static HarmonyLib.Code;

namespace datedliquor.src.BlockClass
{
    internal class BlockDatedLiquorContainer : BlockCorkableLiquidContainer
    {
        public DatedContainableProps CorkedProps = new DatedContainableProps();
/*
 TODO 

Add an attribute to corked containers for the date that they were corked/sealed.

Read that date and compare to the current date, display it in a format defined in the cfg file.

when a bottle is uncorked/uncapped, the date remains displayed on the bottle.

If more liquid is added to the bottle, or the bottle is emptied, then the bottle loses the date attribute.

recorking a partially emptied bottle does not update the date on the container, and the older date is used until emptied or one of the above contitions occur.
 

add the ability to cork/uncork a bottle while it's placed in the world
*/

       
        public override void AddExtraHeldItemInfoPostMaterial(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world)
        {
            base.AddExtraHeldItemInfoPostMaterial(inSlot, dsc, world); 
        }        
    }
}
