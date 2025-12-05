using ACulinaryArtillery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace datedliquor.src.oldstuff
{
    public class BlockCorkedContainer : BlockBottle
    {
        private LiquidTopOpenContainerProps props = new LiquidTopOpenContainerProps();
        /*
         TODO

        Add an attribute to corked containers for the date that they were corked/sealed.

        Read that date and compare to the current date, display it in a format defined in the cfg file.

        when a bottle is uncorked/uncapped, the date remains displayed on the bottle.

        If more liquid is added to the bottle, or the bottle is emptied, then the bottle loses the date attribute.

        recorking a partially emptied bottle does not update the date on the container, and the older date is used until emptied or one of the above contitions occur.
         

        add the ability to cork/uncork a bottle while it's placed in the world
         */


        protected void CorkBottle()
        {
        
        }
        protected void UncorkBottle()
        {

        }

        
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            api.Logger.Event("blockinteractcalled");

            if (byPlayer != null)
            {
                if (blockSel != null && api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGroundStorage blockEntityGroundStorage)
                {
                    ItemSlot slotAt = blockEntityGroundStorage.GetSlotAt(blockSel);
                    
                }



                if (byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Collectible.Code.ToString() == "aculinaryartillery:cork-generic" && byPlayer.Entity.Controls.CtrlKey && IsTopOpened)
                {
                    api.Logger.Event("should cork bottle");
                    return false;
                }
                else if (byPlayer.InventoryManager.ActiveHotbarSlot.Empty && byPlayer.Entity.Controls.CtrlKey)
                {
                    api.Logger.Event("should uncork bottle");
                    return false;
                }
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
