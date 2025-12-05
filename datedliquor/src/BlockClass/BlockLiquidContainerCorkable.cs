using ACulinaryArtillery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace datedliquor.src.BlockClass
{
    public class BlockLiquidContainerCorkable : BlockLiquidContainerTopOpened, IContainedInteractable
    {
        /*
         * 
         * 
         * THIS CLASS IS BASED SOMEWHAT OFF OF BlockBOttle from ACA
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         TODO

        Add an attribute to corked containers for the date that they were corked/sealed.

        Read that date and compare to the current date, display it in a format defined in the cfg file.

        when a bottle is uncorked/uncapped, the date remains displayed on the bottle.

        If more liquid is added to the bottle, or the bottle is emptied, then the bottle loses the date attribute.

        recorking a partially emptied bottle does not update the date on the container, and the older date is used until emptied or one of the above contitions occur.
         

        add the ability to cork/uncork a bottle while it's placed in the world
         */


        private LiquidTopOpenContainerProps props = new LiquidTopOpenContainerProps();

        //Allow previously preset properties to be set from JSON attributes
        public override bool CanDrinkFrom => Attributes["canDrinkFrom"].AsBool(defaultValue: true);
        public override bool IsTopOpened => Attributes["isTopOpened"].AsBool(defaultValue: true);
        public override bool AllowHeldLiquidTransfer => Attributes["allowHeldLiquidTransfer"].AsBool(defaultValue: true);

        protected virtual bool IsClear => Attributes["isClear"].AsBool();

        public virtual float MinFillY => Attributes["minFill"].AsFloat();

        public virtual float MaxFillY => Attributes["maxFill"].AsFloat();
        
        //liquidMaxYTranslate => Props.LiquidMaxYTranslate;
        
        //liquidYTranslatePerLitre => liquidMaxYTranslate / CapacityLitres;
        
        public virtual float MinFillZ => Attributes["minFillSideways"].AsFloat();

        public virtual float MaxFillZ => Attributes["maxFillSideways"].AsFloat();

        public float SatMult => Attributes?["satMult"].AsFloat(1f) ?? 1f;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            props = Attributes?["liquidContainerProps"]?.AsObject(props, Code.Domain) ?? props;
        }
        public override void OnUnloaded(ICoreAPI api)
        {
            if (!(api is ICoreClientAPI coreClientAPI) || !coreClientAPI.ObjectCache.TryGetValue(meshRefsCacheKey, out var value))
            {
                return;
            }

            foreach (KeyValuePair<int, MultiTextureMeshRef> item in (value as Dictionary<int, MultiTextureMeshRef>) ?? new Dictionary<int, MultiTextureMeshRef>())
            {
                item.Value.Dispose();
            }

            coreClientAPI.ObjectCache.Remove(meshRefsCacheKey);
        }

        #region display
        public override byte[]? GetLightHsv(IBlockAccessor blockAccessor, BlockPos pos, ItemStack? stack = null)
        {
            return GetContent(stack)?.Item?.LightHsv ?? ((ThreeBytes)base.GetLightHsv(blockAccessor, pos, stack));
        }
        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            if (!IsClear && !IsTopOpened)
            {
                return;
            }

            object value;
            Dictionary<int, MultiTextureMeshRef> dictionary = (Dictionary<int, MultiTextureMeshRef>)(capi.ObjectCache.TryGetValue(meshRefsCacheKey, out value) ? ((value as Dictionary<int, MultiTextureMeshRef>) ?? new Dictionary<int, MultiTextureMeshRef>()) : (capi.ObjectCache[meshRefsCacheKey] = new Dictionary<int, MultiTextureMeshRef>()));
            ItemStack content = GetContent(itemstack);
            if (content != null)
            {
                int hashCode = (content.StackSize + "x" + content.Collectible.Code.ToShortString()).GetHashCode();
                if (!dictionary.TryGetValue(hashCode, out var value2))
                {
                    value2 = (dictionary[hashCode] = capi.Render.UploadMultiTextureMesh(GenMesh(capi, content)));
                }

                renderinfo.ModelRef = value2;
            }
        }

        public Shape SliceFlattenedShape(Shape fullShape, float fullness, bool isSideways)
        {
            int num = ((!isSideways) ? 1 : 2);
            float num2 = (isSideways ? MinFillZ : MinFillY);
            float num3 = (isSideways ? MaxFillZ : MaxFillY);
            float num4 = num2 + (num3 - num2) * fullness;
            List<ShapeElement> list = new List<ShapeElement>();
            ShapeElement[] elements = fullShape.Elements;
            foreach (ShapeElement shapeElement in elements)
            {
                double num5 = Math.Min(shapeElement.From[num], shapeElement.To[num]);
                double num6 = Math.Max(shapeElement.From[num], shapeElement.To[num]);
                if (num6 < (double)num2 || num5 > (double)num4)
                {
                    continue;
                }

                ShapeElement shapeElement2 = shapeElement.Clone();
                double num7 = Math.Max(shapeElement.From[num], 0.0);
                double num8 = Math.Min(shapeElement.To[num], num4);
                if (!(num7 <= num8))
                {
                    continue;
                }

                shapeElement2.From[num] = num7;
                shapeElement2.To[num] = num8;
                double num9 = num6 - num5;
                double num10 = num8 - num7;
                double num11 = ((num9 > 0.0) ? (num10 / num9) : 0.0);
                for (int j = 0; j < 4; j++)
                {
                    ShapeElementFace shapeElementFace = shapeElement2.FacesResolved[j];
                    if (shapeElementFace != null)
                    {
                        double num12 = shapeElementFace.Uv[1];
                        double num13 = shapeElementFace.Uv[3];
                        double num14 = num13 - num12;
                        shapeElementFace.Uv[1] = (float)(num12 + num14 * (1.0 - num11));
                        shapeElementFace.Uv[3] = (float)num13;
                    }
                }

                if (isSideways)
                {
                    shapeElement2.RotationOrigin = new double[3] { 8.0, 0.2, 8.0 };
                    shapeElement2.RotationY = 180.0;
                }

                list.Add(shapeElement2);
            }

            Shape shape = fullShape.Clone();
            Shape shape2 = shape;
            List<ShapeElement> list2 = list;
            int num15 = 0;
            ShapeElement[] array = new ShapeElement[list2.Count];
            foreach (ShapeElement item in list2)
            {
                array[num15] = item;
                num15++;
            }

            shape2.Elements = array;
            return shape;
        }

        public MeshData? GenMesh(ICoreClientAPI? capi, ItemStack? contentStack, bool isSideways = false, BlockPos? forBlockPos = null)
        {
            IAsset asset = capi?.Assets.TryGet(emptyShapeLoc.CopyWithPathPrefixAndAppendixOnce("shapes/", ".json"));
            if (asset == null)
            {
                return new MeshData();
            }

            capi.Tesselator.TesselateShape(this, asset.ToObject<Shape>(), out var modeldata, new Vec3f(Shape.rotateX, Shape.rotateY, Shape.rotateZ));
            if (contentStack != null && (IsClear || IsTopOpened))
            {
                WaterTightContainableProps containableProps = BlockLiquidContainerBase.GetContainableProps(contentStack);
                if (containableProps != null)
                {
                    float fullness = (float)contentStack.StackSize / containableProps.ItemsPerLitre;
                    Shape shape = capi.Assets.TryGet((containableProps.IsOpaque ? contentShapeLoc : liquidContentShapeLoc).CopyWithPathPrefixAndAppendixOnce("shapes/", ".json"))?.ToObject<Shape>();
                    if (shape == null)
                    {
                        return modeldata;
                    }

                    shape = SliceFlattenedShape(shape.FlattenElementHierarchy(), fullness, isSideways);
                    MeshData sourceMesh = modeldata;
                    capi.Tesselator.TesselateShape("bottle", shape, out modeldata, new BottleTextureSource(capi, contentStack, containableProps.Texture, this), new Vec3f(Shape.rotateX, Shape.rotateY, Shape.rotateZ), 0, 0, 0);
                    for (int i = 0; i < modeldata.Flags.Length; i++)
                    {
                        modeldata.Flags[i] = modeldata.Flags[i] & -4097;
                    }

                    modeldata.AddMeshData(sourceMesh);
                    if (forBlockPos != null)
                    {
                        modeldata.CustomInts = new CustomMeshDataPartInt(modeldata.FlagsCount)
                        {
                            Count = modeldata.FlagsCount
                        };
                        modeldata.CustomInts.Values.Fill(67108864);
                        modeldata.CustomFloats = new CustomMeshDataPartFloat(modeldata.FlagsCount * 2)
                        {
                            Count = modeldata.FlagsCount * 2
                        };
                    }
                }
                else
                {
                    api.Logger.Error($"Bottle with Item {contentStack.Item.Code} does not have waterTightProps and will not render any liquid inside it.");
                    //ACulinaryArtillery.logger?.Error($"Bottle with Item {contentStack.Item.Code} does not have waterTightProps and will not render any liquid inside it.");
                }
            }

            return modeldata;
        }

        public new MeshData? GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas, BlockPos? forBlockPos = null)
        {
            if (forBlockPos != null && GetBlockEntity<BlockEntityBottleRack>(forBlockPos) != null)
            {
                return GenMesh(api as ICoreClientAPI, GetContent(itemstack), isSideways: true, forBlockPos);
            }

            return GenMesh(api as ICoreClientAPI, GetContent(itemstack), isSideways: false, forBlockPos);
        }

        public new string GetMeshCacheKey(ItemStack itemstack)
        {
            ItemStack content = GetContent(itemstack);
            return itemstack.Collectible.Code.ToShortString() + "-" + content?.StackSize + "x" + content?.Collectible.Code.ToShortString();
        }

        #endregion display

        #region interaction

        public virtual bool OnContainedInteractStart(BlockEntityContainer be, ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot activeHotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (activeHotbarSlot.Empty)
            {
                return false;
            }

            Vintagestory.API.Datastructures.JsonObject attributes = activeHotbarSlot.Itemstack.Collectible.Attributes;
            if (attributes != null && attributes.Exists)
            {
                bool flag = false;
                

                slot.MarkDirty();
                be.MarkDirty(redrawOnClient: true);
                if (!(be is BlockEntityGroundStorage))
                {
                    return flag;
                }

                return true;
            }

            return false;
        }

        public bool OnContainedInteractStep(float secondsUsed, BlockEntityContainer be, ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
        {
            return false;
        }

        public void OnContainedInteractStop(float secondsUsed, BlockEntityContainer be, ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
        {

        }

        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            IPlayer player = (byEntity as EntityPlayer)?.Player;
            if (player != null && blockSel == null && entitySel == null && Variant["type"] == "corked")
            {
                ItemSlot itemSlot = player.InventoryManager?.OffhandHotbarSlot;
                if (itemSlot != null && (itemSlot.Empty || itemSlot.Itemstack.Collectible.FirstCodePart() == "cork"))
                {
                    ItemStack itemstack = new ItemStack(byEntity.World.GetBlock(CodeWithVariant("type", "fired")))
                    {
                        Attributes = itemslot.Itemstack.Attributes
                    };
                    if (itemslot.StackSize == 1)
                    {
                        itemslot.Itemstack = itemstack;
                    }
                    else
                    {
                        itemslot.TakeOut(1);
                        itemslot.MarkDirty();
                        if (!player.InventoryManager.TryGiveItemstack(itemstack, slotNotifyEffect: true))
                        {
                            byEntity.World.SpawnItemEntity(itemstack, byEntity.Pos.AsBlockPos);
                        }
                    }

                    ItemStack itemStack = new ItemStack(byEntity.World.GetItem("aculinaryartillery:cork-generic"));
                    if (new DummySlot(itemStack).TryPutInto(byEntity.World, itemSlot) <= 0)
                    {
                        byEntity.World.SpawnItemEntity(itemStack, byEntity.Pos.AsBlockPos);
                    }

                    handHandling = EnumHandHandling.PreventDefault;
                    return;
                }
            
                (api as ICoreClientAPI)?.TriggerIngameError(this, "fulloffhandslot", Lang.Get("aculinaryartillery:bottle-fulloffhandslot"));
            }

            base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
        }
        public override void TryMergeStacks(ItemStackMergeOperation op)
        {
            ItemSlot sourceSlot = op.SourceSlot;

            if (Variant["type"] != "corked" && op.CurrentPriority == EnumMergePriority.DirectMerge)
            {
                Vintagestory.API.Datastructures.JsonObject itemAttributes = sourceSlot.Itemstack.ItemAttributes;
                if (itemAttributes != null && itemAttributes["canSealBottle"]?.AsBool() == true)
                {
                    ItemSlot sinkSlot = op.SinkSlot;
                    ItemStack itemstack = new ItemStack(op.World.GetBlock(sinkSlot.Itemstack.Collectible.CodeWithVariant("type", "corked")))
                    {
                        Attributes = sinkSlot.Itemstack.Attributes
                    };
                    if (sinkSlot.StackSize == 1)
                    {
                        sinkSlot.Itemstack = itemstack;
                    }
                    else
                    {
                        sinkSlot.TakeOut(1);
                        if (!op.ActingPlayer.InventoryManager.TryGiveItemstack(itemstack, slotNotifyEffect: true))
                        {
                            op.World.SpawnItemEntity(itemstack, op.ActingPlayer.Entity.Pos.AsBlockPos);
                        }
                    }

                    op.MovedQuantity = 1;
                    sourceSlot.TakeOut(1);
                    sinkSlot.MarkDirty();
                    return;
                }
            }

            base.TryMergeStacks(op);
        }
        public override int GetMergableQuantity(ItemStack sinkStack, ItemStack sourceStack, EnumMergePriority priority)
        {
            if (priority == EnumMergePriority.DirectMerge)
            {
                Vintagestory.API.Datastructures.JsonObject itemAttributes = sourceStack.ItemAttributes;
                if (itemAttributes != null && itemAttributes["canSealBottle"]?.AsBool() == true && Variant["type"] != "corked")
                {
                    return 1;
                }
            }

            return base.GetMergableQuantity(sinkStack, sourceStack, priority);
        }

       
        //Need to write methods that handle the corking/uncorking of a bottle so we can override them in the dependent class. Will return false if unable to cork bottle
        protected void CorkContainer(ItemSlot containerSlot, ItemSlot corkSlot, EntityAgent byEntity)
        {
            IPlayer player = (byEntity as EntityPlayer)?.Player;
            if (Variant["type"] != "corked")
            {
                Vintagestory.API.Datastructures.JsonObject itemAttributes = corkSlot.Itemstack.ItemAttributes;
                if (itemAttributes != null && itemAttributes["canSealBottle"]?.AsBool() == true)
                {
                    ItemStack itemstack = new ItemStack(byEntity.World.GetBlock(containerSlot.Itemstack.Collectible.CodeWithVariant("type", "corked")))
                    {
                        Attributes = containerSlot.Itemstack.Attributes
                    };
                    if (containerSlot.StackSize == 1)
                    {
                        containerSlot.Itemstack = itemstack;
                    }
                    else
                    {
                        containerSlot.TakeOut(1);
                        if (player.InventoryManager.TryGiveItemstack(itemstack, slotNotifyEffect: true))
                        {
                            byEntity.World.SpawnItemEntity(itemstack, byEntity.Pos.AsBlockPos);
                        }
                    }

                    
                    corkSlot.TakeOut(1);
                    containerSlot.MarkDirty();
                    return;
                }
            }
        }
        protected void OnUncorkContainer(ItemStack containerstack, ItemStack corkStack, EntityAgent byEntity)
        {

        }

        #endregion interaction

        #region nutrition
        protected override bool tryEatStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, ItemStack? content = null)
        {
            if (GetNutritionProperties(byEntity.World, slot.Itemstack, byEntity) == null)
            {
                return false;
            }

            Vec3d xYZ = byEntity.Pos.AheadCopy(0.40000000596046448).XYZ;
            xYZ.X += byEntity.LocalEyePos.X;
            xYZ.Y += byEntity.LocalEyePos.Y - 0.40000000596046448;
            xYZ.Z += byEntity.LocalEyePos.Z;
            if (secondsUsed > 0.5f && (int)(30f * secondsUsed) % 7 == 1)
            {
                byEntity.World.SpawnCubeParticles(xYZ, GetContent(slot.Itemstack), 0.3f, 4, 0.5f, (byEntity as EntityPlayer)?.Player);
            }

            if (byEntity.World is IClientWorldAccessor)
            {
                ModelTransform modelTransform = new ModelTransform();
                modelTransform.EnsureDefaultValues();
                modelTransform.Origin.Set(0f, 0f, 0f);
                if (secondsUsed > 0.5f)
                {
                    modelTransform.Translation.Y = Math.Min(0.02f, GameMath.Sin(20f * secondsUsed) / 10f);
                }

                modelTransform.Translation.X -= Math.Min(1f, secondsUsed * 4f * 1.57f);
                modelTransform.Translation.Y -= Math.Min(0.05f, secondsUsed * 2f);
                modelTransform.Rotation.X += Math.Min(30f, secondsUsed * 350f);
                modelTransform.Rotation.Y += Math.Min(80f, secondsUsed * 350f);
                return secondsUsed <= 1f;
            }

            return true;
        }

        protected override void tryEatStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity)
        {
            if (secondsUsed < 0.95f || !(byEntity.World is IServerWorldAccessor))
            {
                return;
            }

            FoodNutritionProperties nutritionProperties = GetNutritionProperties(byEntity.World, slot.Itemstack, byEntity);
            if (nutritionProperties != null)
            {
                float currentLitres = GetCurrentLitres(slot.Itemstack);
                float litresToDrink = ((currentLitres >= 0.25f) ? 0.25f : currentLitres);
                TransitionState transitionState = UpdateAndGetTransitionState(api.World, slot, EnumTransitionType.Perish);
                float num = ((currentLitres == 1f) ? 0.25f : (((double)currentLitres == 0.75) ? 0.3333f : (((double)currentLitres == 0.5) ? 0.5f : 1f)));
                byEntity.ReceiveSaturation(nutritionProperties.Satiety * num * GlobalConstants.FoodSpoilageSatLossMul(transitionState?.TransitionLevel ?? 0f, slot.Itemstack, byEntity), nutritionProperties.FoodCategory);
                IPlayer player = (byEntity as EntityPlayer)?.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                SplitStackAndPerformAction(byEntity, slot, (ItemStack stack) => TryTakeLiquid(stack, litresToDrink)?.StackSize ?? 0);
                if (nutritionProperties.Intoxication > 0f)
                {
                    float num2 = byEntity.WatchedAttributes.GetFloat("intoxication");
                    byEntity.WatchedAttributes.SetFloat("intoxication", Math.Min(litresToDrink, num2 + nutritionProperties.Intoxication * num));
                }

                float num3 = nutritionProperties.Health * num * GlobalConstants.FoodSpoilageHealthLossMul(transitionState?.TransitionLevel ?? 0f, slot.Itemstack, byEntity);
                if (num3 != 0f)
                {
                    byEntity.ReceiveDamage(new DamageSource
                    {
                        Source = EnumDamageSource.Internal,
                        Type = ((num3 > 0f) ? EnumDamageType.Heal : EnumDamageType.Poison)
                    }, Math.Abs(num3));
                }

                slot.MarkDirty();
                player?.InventoryManager.BroadcastHotbarSlot();
                if (GetCurrentLitres(slot.Itemstack) == 0f)
                {
                    SetContent(slot.Itemstack, null);
                }
            }
        }
        public FoodNutritionProperties[]? GetPropsFromArray(float[]? satieties)
        {
            if (satieties == null || satieties.Length < 6)
            {
                return null;
            }

            List<FoodNutritionProperties> list = new List<FoodNutritionProperties>();
            for (int i = 1; i <= 5; i++)
            {
                if (satieties[i] != 0f)
                {
                    list.Add(new FoodNutritionProperties
                    {
                        FoodCategory = (EnumFoodCategory)(i - 1),
                        Satiety = satieties[i] * SatMult
                    });
                }
            }

            if (satieties[0] != 0f && list.Count > 0)
            {
                list[0].Health = satieties[0] * SatMult;
            }

            List<FoodNutritionProperties> list2 = list;
            int num = 0;
            FoodNutritionProperties[] array = new FoodNutritionProperties[list2.Count];
            foreach (FoodNutritionProperties item in list2)
            {
                array[num] = item;
                num++;
            }

            return array;
        }

        #endregion nutrition

        #region information
        public new string GetContainedInfo(ItemSlot inSlot)
        {
            float currentLitres = GetCurrentLitres(inSlot.Itemstack);
            ItemStack content = GetContent(inSlot.Itemstack);
            if (content == null || currentLitres <= 0f)
            {
                return Lang.GetWithFallback("contained-empty-container", "{0} (Empty)", inSlot.Itemstack.GetName());
            }

            string text = Lang.Get(content.Collectible.Code.Domain + ":incontainer-" + content.Class.ToString().ToLowerInvariant() + "-" + content.Collectible.Code.Path);
            return Lang.Get("contained-liquidcontainer-compact", inSlot.Itemstack.GetName(), currentLitres, text, PerishableInfoCompactContainer(api, inSlot));
        }

        public new string GetContainedName(ItemSlot inSlot, int quantity)
        {
            return inSlot.Itemstack.GetName();
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
            ItemStack content = GetContent(inSlot.Itemstack);
            if (content == null)
            {
                return;
            }

            string text = content.Collectible.Code.Domain + ":itemdesc-" + content.Collectible.Code.Path;
            string matching = Lang.GetMatching(text);
            DummySlot inSlot2 = new DummySlot(content);
            if (matching != text)
            {
                dsc.AppendLine();
                dsc.Append(matching);
            }

            EntityPlayer entityPlayer = (world as IClientWorldAccessor)?.Player.Entity;
            float spoilState = AppendPerishableInfoText(inSlot2, new StringBuilder(), world);
            FoodNutritionProperties[] expandedContentNutritionProperties = ItemExpandedRawFood.GetExpandedContentNutritionProperties(world, inSlot2, content, entityPlayer);
            FoodNutritionProperties[] propsFromArray = GetPropsFromArray((content.Attributes["expandedSats"] as FloatArrayAttribute)?.value);
            if (expandedContentNutritionProperties == null || propsFromArray == null || propsFromArray.Length == 0)
            {
                return;
            }

            dsc.AppendLine();
            dsc.AppendLine(Lang.Get("efrecipes:Extra Nutrients"));
            FoodNutritionProperties[] array = propsFromArray;
            foreach (FoodNutritionProperties foodNutritionProperties in array)
            {
                double num = content.StackSize;
                float num2 = GlobalConstants.FoodSpoilageSatLossMul(spoilState, content, entityPlayer);
                float num3 = GlobalConstants.FoodSpoilageHealthLossMul(spoilState, content, entityPlayer);
                if (Math.Abs(foodNutritionProperties.Health * num3) > 0.001f)
                {
                    dsc.AppendLine(Lang.Get("efrecipes:- {0} {2} sat, {1} hp", Math.Round((double)(foodNutritionProperties.Satiety * num2) * (num / 10.0), 1), (double)(foodNutritionProperties.Health * num3) * (num / 10.0), foodNutritionProperties.FoodCategory.ToString()));
                }
                else
                {
                    dsc.AppendLine(Lang.Get("efrecipes:- {0} {1} sat", Math.Round((double)(foodNutritionProperties.Satiety * num2) * (num / 10.0)), foodNutritionProperties.FoodCategory.ToString()));
                }
            }
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[4]
            {
            new WorldInteraction
            {
                ActionLangCode = "heldhelp-empty",
                HotKeyCode = "sprint",
                MouseButton = EnumMouseButton.Right,
                ShouldApply = (WorldInteraction wi, BlockSelection bs, EntitySelection es) => GetCurrentLitres(inSlot.Itemstack) > 0f
            },
            new WorldInteraction
            {
                ActionLangCode = "aculinaryartillery:heldhelp-drink",
                MouseButton = EnumMouseButton.Right,
                ShouldApply = delegate
                {
                    string text = GetContent(inSlot.Itemstack)?.GetName();
                    return text != null && !(text == "Water") && GetCurrentLitres(inSlot.Itemstack) > 0f;
                }
            },
            new WorldInteraction
            {
                ActionLangCode = "heldhelp-fill",
                MouseButton = EnumMouseButton.Right,
                ShouldApply = delegate(WorldInteraction wi, BlockSelection bs, EntitySelection es)
                {
                    int result;
                    if (bs != null)
                    {
                        Block block = api.World.BlockAccessor.GetBlock(bs.Position.AddCopy(bs.Face));
                        if (block != null && block.Code.GetName().Contains("water-"))
                        {
                            result = ((GetCurrentLitres(inSlot.Itemstack) == 0f) ? 1 : 0);
                            goto IL_006b;
                        }
                    }
                    result = 0;
                    goto IL_006b;
                    IL_006b:
                    return (byte)result != 0;
                }
            },
            new WorldInteraction
            {
                ActionLangCode = "heldhelp-place",
                HotKeyCode = "sneak",
                MouseButton = EnumMouseButton.Right,
                ShouldApply = (WorldInteraction wi, BlockSelection bs, EntitySelection es) => true
            }
            };
        }
        #endregion information
    }
}
