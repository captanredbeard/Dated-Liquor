using datedliquor.src.Behaviors;
using datedliquor.src.Harmony;
using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace datedliquor
{
    public class datedliquorModSystem : ModSystem
    {
        Harmony harmonyInstance;
        public override void Start(ICoreAPI api)
        {
            var modid = Mod.Info.ModID;
            harmonyInstance = new Harmony(modid);

            api.RegisterCollectibleBehaviorClass(modid+":AlcoholInfo", typeof(CollectibleBehaviorAlcoholInfo));

            //harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockEntityCondenser).GetMethod("TryPut", BindingFlags.NonPublic | BindingFlags.Instance),
              // postfix: new HarmonyMethod(typeof(harmPatches).GetMethod("Postfix_BlockEntityAnvil_TryPut")));

            harmonyInstance.Patch(typeof(Vintagestory.GameContent.BlockEntityCondenser).GetMethod("TryTake", BindingFlags
                .NonPublic | BindingFlags.Instance),
                prefix: new HarmonyMethod(typeof(harmPatches).GetMethod("Prefix_BlockEntityAnvil_TryTake")));
        }
        public override void StartClientSide(ICoreClientAPI api)
        {
            harmonyInstance = new Harmony(Mod.Info.ModID);
            harmonyInstance.Patch(typeof(CollectibleObject).GetMethod("GetHeldItemInfo"),
                postfix: new HarmonyMethod(typeof(harmPatches).GetMethod("Postfix_GetHeldItemInfo")));

        }
    }
}
