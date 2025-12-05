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
    internal class BlockDatedLiquorContainer : BlockLiquidContainerCorkable
    {
        public CorkedContainableProps CorkedProps = new CorkedContainableProps();



        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            CorkedProps = Attributes?["liquidContainerProps"]?.AsObject(CorkedProps, Code.Domain) ?? CorkedProps;
        }



        public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            base.OnCreatedByCrafting(allInputslots, outputSlot, byRecipe);
            if (this.Code.EndVariant() == "corked")
            {

                CorkedProps.LastCorkedHours = api.World.Calendar.ElapsedHours;
            }
            else 
            {
                api.Logger.Event("IsTOpOpened"+IsTopOpened);
                if (IsTopOpened)
                {
                    
                }
            }
        }
        public override void OnConsumedByCrafting(ItemSlot[] allInputSlots, ItemSlot stackInSlot, GridRecipe gridRecipe, CraftingRecipeIngredient fromIngredient, IPlayer byPlayer, int quantity)
        {
            base.OnConsumedByCrafting(allInputSlots, stackInSlot, gridRecipe, fromIngredient, byPlayer, quantity);
        }



    }
}
