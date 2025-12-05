using datedliquor.src.BlockClass;
using datedliquor.src.EntityClass;
using datedliquor.src.ItemClass;
using datedliquor.src.oldshit;
using datedliquor.src.oldstuff;
using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace datedliquor
{
    public class datedliquorModSystem : ModSystem
    {
        /*
         * Setup basic yaml config via configlib or autoconfiglib
         * https://mods.vintagestory.at/configlib
         * https://mods.vintagestory.at/autoconfiglib
         *
         * 
         * Consider whether to rewrite ACA's BlockBottle to make our bottles a bit easier to code.
         * 
         * Look at transitionable properties for use in the dating system and get it displaying properly.
         * 
         * add cork/uncork methods for recipes and other interactions to call when bottles are corked/uncorked.
         * 
         * make sure to leave room for the future recipe system implementation.
         * 
         * 
         * 
         * 
        */
        Harmony harmonyInstance;
        public override void Start(ICoreAPI api)
        {
            var modid = Mod.Info.ModID;
            api.RegisterBlockClass(modid + ".BlockLiquidContainerCorkable", typeof(BlockLiquidContainerCorkable));
            api.RegisterBlockClass(modid + ".BlockDatedLiquorContainer", typeof(BlockDatedLiquorContainer));

            api.RegisterBlockClass(modid + ".BlockLiquidContainerCorkable", typeof(BlockLiquidContainerCorkable));



            api.RegisterBlockClass(modid + ".BlockThrowableBottle", typeof(BlockThrowableBottle));
            api.RegisterEntity(modid + ".EntityThrownBottle", typeof(EntityThrownBottle));
            api.RegisterItemClass(modid + ".ItemShards", typeof(ItemShards));


            harmonyInstance = new Harmony(modid);
            //harmonyInstance.PatchAll();
        }
        public override void StartClientSide(ICoreClientAPI api)
        {

        }
        public override void StartServerSide(ICoreServerAPI api)
        {

        }
        public override void AssetsFinalize(ICoreAPI api)
        {

        }
        public override void Dispose()
        {
            base.Dispose();
            harmonyInstance.UnpatchAll(Mod.Info.ModID);
        }
    }
}
