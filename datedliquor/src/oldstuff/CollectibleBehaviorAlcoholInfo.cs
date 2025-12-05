using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Cairo;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace datedliquor.src.oldshit
{
    public class CollectibleBehaviorAlcoholInfo : CollectibleBehavior
    {
        private ICoreAPI Api;

        public ExtraHandbookSection[] ExtraHandBookSections;

        public CollectibleBehaviorAlcoholInfo(CollectibleObject collObj)
        : base(collObj)
        {
        }
        public override void OnLoaded(ICoreAPI api)
        {
            Api = api;
            JsonObject jsonObject = collObj.Attributes?["handbook"]?["extraSections"];
            if (jsonObject != null && jsonObject.Exists)
            {
                ExtraHandBookSections = jsonObject?.AsObject<ExtraHandbookSection[]>();
            }
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
            dsc.Append("Alcohol Info Behavior Test Text");
        }
        
        public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe, ref EnumHandling bhHandling)
        {
            
            base.OnCreatedByCrafting(allInputslots, outputSlot, byRecipe, ref bhHandling);
        }

        public override ItemStack OnTransitionNow(ItemSlot slot, TransitionableProperties props, ref EnumHandling handling)
        {
            return base.OnTransitionNow(slot, props, ref handling);
        }
    }
}
