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
    public class BlockDatedLiquorContainer : BlockCorkableLiquidContainer
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

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            api.Logger.Event("OnHeldInteractStop");

            // Store content state before base call
            ItemStack contentBefore = GetContent(slot.Itemstack);
            float litresBefore = GetCurrentLitres(slot.Itemstack);

            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);

            // Check if content changed after interaction (filling/emptying occurred)
            ItemStack contentAfter = GetContent(slot.Itemstack);
            float litresAfter = GetCurrentLitres(slot.Itemstack);

            IPlayer player = (byEntity as EntityPlayer)?.Player;

            // If bottle was empty and now has content, stamp it
            if ((contentBefore == null || litresBefore <= 0f) && (contentAfter != null && litresAfter > 0f))
            {
                CheckAndStampIfFilled(slot.Itemstack, player);
                slot.MarkDirty();
            }
            // If bottle is being emptied, clear bottling info
            else if ((contentBefore != null && litresBefore > 0f) && (contentAfter == null || litresAfter <= 0f))
            {
                ClearBottlingInfo(slot.Itemstack);
                slot.MarkDirty();
            }
        }

        //You'll be able to use these to run custom logic when corking/uncorking the bottle
        protected override void OnCorkContainer(ItemSlot containerSlot, EntityAgent byEntity)
        {
            base.OnCorkContainer(containerSlot, byEntity);
            CheckAndStampIfFilled(containerSlot.Itemstack,byEntity as IPlayer);
        }
        protected override void OnUncorkContainer(ItemSlot containerSlot, EntityAgent byEntity)
        {
            base.OnUncorkContainer(containerSlot, byEntity);
            ClearBottlingInfo(containerSlot.Itemstack);
        }


        protected override void tryEatStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity)
        {
            base.tryEatStop(secondsUsed, slot, byEntity);
            if (slot.Itemstack == null)
            {
                ClearBottlingInfo(slot.Itemstack);
            }

        }











        protected void StampFilled(ItemStack containerStack, IPlayer byPlayer = null)
        {
            if (containerStack == null) return;
            var attrs = containerStack.Attributes;
            if (attrs == null) return;

            // Store bottler name if player is provided
            if (byPlayer != null)
            {
                attrs.SetString("bottledBy", byPlayer.PlayerName);
            }

            // Store game world time (in total days) when bottled
            // This uses the world's calendar time, which is more meaningful for gameplay
            if (api.World is IServerWorldAccessor serverWorld)
            {
                attrs.SetDouble("bottledOnTotalDays", serverWorld.Calendar.TotalDays);
            }
            else if (api.World is IClientWorldAccessor clientWorld)
            {
                attrs.SetDouble("bottledOnTotalDays", clientWorld.Calendar.TotalDays);
            }

            // Also store UTC timestamp as backup
            attrs.SetLong("filledUtcTicks", DateTime.UtcNow.Ticks);
        }

        protected void ClearBottlingInfo(ItemStack containerStack)
        {
            if (containerStack == null) return;
            var attrs = containerStack.Attributes;
            if (attrs == null) return;

            attrs.RemoveAttribute("bottledBy");
            attrs.RemoveAttribute("bottledOnTotalDays");
            attrs.RemoveAttribute("filledUtcTicks");
        }

        /// <summary>
        /// Checks if a bottle was just filled (transitioned from empty to having content)
        /// and stamps it with bottling information
        /// </summary>
        protected void CheckAndStampIfFilled(ItemStack containerStack, IPlayer byPlayer = null)
        {
            if (containerStack == null) return;

            ItemStack currentContent = GetContent(containerStack);
            float currentLitres = GetCurrentLitres(containerStack);

            // Check if bottle was previously empty (no bottling info) and now has content
            var attrs = containerStack.Attributes;
            bool wasEmpty = attrs?.GetString("bottledBy") == null && currentLitres > 0f;

            // If bottle is being filled for the first time (has content but no bottling info)
            if (currentContent != null && currentLitres > 0f && wasEmpty)
            {
                StampFilled(containerStack, byPlayer);
            }
            // If bottle is being emptied, clear bottling info
            else if (currentContent == null || currentLitres <= 0f)
            {
                ClearBottlingInfo(containerStack);
            }
        }
    }
}
