using ACulinaryArtillery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using HarmonyLib;

namespace datedliquor.src.Harmony
{
    [HarmonyPatch(typeof(BlockEntityBottleRack),"OnInteract")]
    public static class ACABottleRackPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(IPlayer byPlayer, BlockSelection blockSel, BlockEntityBottleRack __instance, ref bool __result)
        {
            var playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            
                var colObj = playerSlot.Itemstack.Collectible;

                BlockBottle bottle = colObj as BlockBottle;
                
                //Check if Corner rack and bottle being placed is a winebottle
                if(__instance.Block.Code.PathStartsWith("bottlerackcorner") && bottle.Code.PathStartsWith("winebottle"))
                {
                    __instance.Api.Logger.Error("blocksel:"+blockSel.SelectionBoxIndex);
                    var index = blockSel.SelectionBoxIndex;

               // var sideslots = __instance.Inventory.GetSlotsIfExists(byPlayer, [__instance.Inventory.InventoryID], [index - 1, index + 1]);
               
                
                if (false)
                {
                    __instance.Api.Logger.Event("inventory:", string.Join(",", __instance.Inventory));



                    //Run fullness checks from base method
                    float fullness = bottle?.GetCurrentLitres(playerSlot.Itemstack) ?? 0;
                    if (bottle?.IsTopOpened == true && fullness > 0.2f)
                    {
                        (__instance.Api as ICoreClientAPI)?.TriggerIngameError(__instance, "bottletoofull", Lang.Get("aculinaryartillery:bottle-toofullforrack"));
                        __result = false;
                        return true;
                    }
                }
            }
            return true;
        }
    }
}
