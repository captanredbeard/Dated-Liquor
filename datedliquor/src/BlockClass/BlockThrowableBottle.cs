using Cairo;
using datedliquor.src.EntityClass;
using datedliquor.src.oldstuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static System.Net.Mime.MediaTypeNames;

namespace datedliquor.src.BlockClass
{
    internal class BlockThrowableBottle : BlockLiquidContainerCorkable
    {
        private SkillItem[] toolModes;

        private float damage;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            ICoreClientAPI capi = api as ICoreClientAPI;
            if (capi != null)
            {
                toolModes = ObjectCacheUtil.GetOrCreate(api, "throwableBottleToolModes", () => new SkillItem[2]
                {
                new SkillItem
                {
                    Code = new AssetLocation("drink"),
                    Name = Lang.Get("Normal Behavior mode (drinking, pouring, etc)")
                }.WithIcon(capi, Drawcreate1_svg),
                new SkillItem
                {
                    Code = new AssetLocation("throw"),
                    Name = Lang.Get("Set to throw your glass at someone")
                }.WithIcon(capi, Drawcreate4_svg)
                });
            }
        }

        public override void OnUnloaded(ICoreAPI api)
        {
            int num = 0;
            while (toolModes != null && num < toolModes.Length)
            {
                toolModes[num]?.Dispose();
                num++;
            }
        }

        #region throwing
        public override string GetHeldTpUseAnimation(ItemSlot activeHotbarSlot, Entity byEntity)
        {
            return null;
        }

        public override string GetHeldTpHitAnimation(ItemSlot slot, Entity byEntity)
        {
            
            return base.GetHeldTpHitAnimation(slot, byEntity);
        }

        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (GetToolMode(itemslot, (byEntity as EntityPlayer).Player, blockSel) == 1)
            {
                Block block = ((blockSel == null) ? null : byEntity.World.BlockAccessor.GetBlock(blockSel.Position));
                if (block is BlockDisplayCase || block is BlockSign)
                {
                    base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handling);
                    handling = EnumHandHandling.NotHandled;
                    return;
                }

                EnumHandHandling handHandling = EnumHandHandling.NotHandled;
                CollectibleBehavior[] collectibleBehaviors = CollectibleBehaviors;
                foreach (CollectibleBehavior obj in collectibleBehaviors)
                {
                    EnumHandling handling2 = EnumHandling.PassThrough;
                    obj.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling2);
                    if (handling2 == EnumHandling.PreventSubsequent)
                    {
                        break;
                    }
                }
                if (!byEntity.Controls.ShiftKey)
                {
                    byEntity.Attributes.SetInt("aiming", 1);
                    byEntity.Attributes.SetInt("aimingCancel", 0);
                    byEntity.StartAnimation("aim");
                    handling = EnumHandHandling.PreventDefault;
                }
            }
            else
            {
                base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            }
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            bool flag = true;
            bool flag2 = false;
            CollectibleBehavior[] collectibleBehaviors = CollectibleBehaviors;
            foreach (CollectibleBehavior obj in collectibleBehaviors)
            {
                EnumHandling handling = EnumHandling.PassThrough;
                bool flag3 = obj.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel, ref handling);
                if (handling != EnumHandling.PassThrough)
                {
                    flag = flag && flag3;
                    flag2 = true;
                }

                if (handling == EnumHandling.PreventSubsequent)
                {
                    return flag;
                }
            }

            if (flag2)
            {
                return flag;
            }

            if (byEntity.Attributes.GetInt("aimingCancel") == 1)
            {
                return false;
            }

            if (byEntity.World is IClientWorldAccessor)
            {
                ModelTransform modelTransform = new ModelTransform();
                modelTransform.EnsureDefaultValues();
                float num = GameMath.Clamp(secondsUsed * 3f, 0f, 1.5f);
                modelTransform.Translation.Set(num / 4f, num / 2f, 0f);
                modelTransform.Rotation.Set(0f, 0f, GameMath.Min(90f, secondsUsed * 360f / 1.5f));
            }

            return true;
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            if (GetToolMode(slot, (byEntity as EntityPlayer).Player, blockSel) == 1)
            {
                byEntity.Attributes.SetInt("aiming", 0);
                byEntity.StopAnimation("aim");
                if (cancelReason != EnumItemUseCancelReason.ReleasedMouse)
                {
                    byEntity.Attributes.SetInt("aimingCancel", 1);
                }

                return true;
            }
            return base.OnHeldInteractCancel(secondsUsed, slot, byEntity, blockSel, entitySel, cancelReason);
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (GetToolMode(slot, (byEntity as EntityPlayer).Player, blockSel) == 1)
            {
                bool flag = false;
                CollectibleBehavior[] collectibleBehaviors = CollectibleBehaviors;
                foreach (CollectibleBehavior obj in collectibleBehaviors)
                {
                    EnumHandling handling = EnumHandling.PassThrough;
                    obj.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel, ref handling);
                    if (handling != EnumHandling.PassThrough)
                    {
                        flag = true;
                    }

                    if (handling == EnumHandling.PreventSubsequent)
                    {
                        return;
                    }
                }

                if (flag || byEntity.Attributes.GetInt("aimingCancel") == 1)
                {
                    return;
                }

                byEntity.Attributes.SetInt("aiming", 0);
                byEntity.StopAnimation("aim");
                if (!(secondsUsed < 0.35f))
                {
                    ItemStack projectileStack = slot.TakeOut(1);
                    slot.MarkDirty();
                    IPlayer dualCallByPlayer = null;
                    if (byEntity is EntityPlayer)
                    {
                        dualCallByPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                    }

                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/player/throw"), byEntity, dualCallByPlayer, randomizePitch: false, 8f);

                    EntityProperties entityType = byEntity.World.GetEntityType(this.Code);

                    Entity entity = byEntity.World.ClassRegistry.CreateEntity(entityType);
                    ((EntityThrownBottle)entity).FiredBy = byEntity;
                    ((EntityThrownBottle)entity).Damage = damage;
                    ((EntityThrownBottle)entity).ProjectileStack = projectileStack;
                    EntityProjectile.SpawnThrownEntity(entity, byEntity, 0.75, 0.1, 0.2);
                    byEntity.StartAnimation("throw");
                }
            }
            base.OnHeldInteractStop(secondsUsed,slot, byEntity,blockSel,entitySel);
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
            dsc.AppendLine(Lang.Get("{0} blunt damage when thrown", damage));
        }
        #endregion throwing
        public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
        {
            return toolModes;
        }
        public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
        {
            return slot.Itemstack.Attributes.GetInt("toolMode");
        }
        public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel, int toolMode)
        {
            slot.Itemstack.Attributes.SetInt("toolMode", toolMode);
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[1]
            {
            new WorldInteraction
            {
                ActionLangCode = "Change bottle mode",
                HotKeyCodes = new string[1] { "toolmodeselect" },
                MouseButton = EnumMouseButton.None
            }
            };
        }


        #region svg
        public static void Drawcreate1_svg(Context cr, int x, int y, float width, float height, double[] rgba)
        {
            Pattern pattern = null;
            Matrix matrix = cr.Matrix;
            cr.Save();
            float num = 129f;
            float num2 = 129f;
            float num3 = Math.Min(width / num, height / num2);
            matrix.Translate((float)x + Math.Max(0f, (width - num * num3) / 2f), (float)y + Math.Max(0f, (height - num2 * num3) / 2f));
            matrix.Scale(num3, num3);
            cr.Matrix = matrix;
            cr.Operator = Operator.Over;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(51.828125, 51.828125);
            cr.LineTo(76.828125, 51.828125);
            cr.LineTo(76.828125, 76.828125);
            cr.LineTo(51.828125, 76.828125);
            cr.ClosePath();
            cr.MoveTo(51.828125, 51.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.FillRule = FillRule.Winding;
            cr.FillPreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(51.828125, 51.828125);
            cr.LineTo(76.828125, 51.828125);
            cr.LineTo(76.828125, 76.828125);
            cr.LineTo(51.828125, 76.828125);
            cr.ClosePath();
            cr.MoveTo(51.828125, 51.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Restore();
        }

        public void Drawremove1_svg(Context cr, int x, int y, float width, float height, double[] rgba)
        {
            Pattern pattern = null;
            Matrix matrix = cr.Matrix;
            cr.Save();
            float num = 129f;
            float num2 = 129f;
            float num3 = Math.Min(width / num, height / num2);
            matrix.Translate((float)x + Math.Max(0f, (width - num * num3) / 2f), (float)y + Math.Max(0f, (height - num2 * num3) / 2f));
            matrix.Scale(num3, num3);
            cr.Matrix = matrix;
            cr.Operator = Operator.Over;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(51.828125, 51.828125);
            cr.LineTo(76.828125, 51.828125);
            cr.LineTo(76.828125, 76.828125);
            cr.LineTo(51.828125, 76.828125);
            cr.ClosePath();
            cr.MoveTo(51.828125, 51.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.FillRule = FillRule.Winding;
            cr.FillPreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(51.828125, 51.828125);
            cr.LineTo(76.828125, 51.828125);
            cr.LineTo(76.828125, 76.828125);
            cr.LineTo(51.828125, 76.828125);
            cr.ClosePath();
            cr.MoveTo(51.828125, 51.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(91.828125, 36.828125);
            cr.LineTo(36.328125, 92.328125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Restore();
        }

        public static void Drawcreate4_svg(Context cr, int x, int y, float width, float height, double[] rgba)
        {
            Pattern pattern = null;
            Matrix matrix = cr.Matrix;
            cr.Save();
            float num = 129f;
            float num2 = 129f;
            float num3 = Math.Min(width / num, height / num2);
            matrix.Translate((float)x + Math.Max(0f, (width - num * num3) / 2f), (float)y + Math.Max(0f, (height - num2 * num3) / 2f));
            matrix.Scale(num3, num3);
            cr.Matrix = matrix;
            cr.Operator = Operator.Over;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(34.078125, 33.828125);
            cr.LineTo(59.078125, 33.828125);
            cr.LineTo(59.078125, 58.828125);
            cr.LineTo(34.078125, 58.828125);
            cr.ClosePath();
            cr.MoveTo(34.078125, 33.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.FillRule = FillRule.Winding;
            cr.FillPreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(34.078125, 33.828125);
            cr.LineTo(59.078125, 33.828125);
            cr.LineTo(59.078125, 58.828125);
            cr.LineTo(34.078125, 58.828125);
            cr.ClosePath();
            cr.MoveTo(34.078125, 33.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(71.578125, 33.828125);
            cr.LineTo(96.578125, 33.828125);
            cr.LineTo(96.578125, 58.828125);
            cr.LineTo(71.578125, 58.828125);
            cr.ClosePath();
            cr.MoveTo(71.578125, 33.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.FillRule = FillRule.Winding;
            cr.FillPreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(71.578125, 33.828125);
            cr.LineTo(96.578125, 33.828125);
            cr.LineTo(96.578125, 58.828125);
            cr.LineTo(71.578125, 58.828125);
            cr.ClosePath();
            cr.MoveTo(71.578125, 33.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(34.078125, 71.828125);
            cr.LineTo(59.078125, 71.828125);
            cr.LineTo(59.078125, 96.828125);
            cr.LineTo(34.078125, 96.828125);
            cr.ClosePath();
            cr.MoveTo(34.078125, 71.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.FillRule = FillRule.Winding;
            cr.FillPreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(34.078125, 71.828125);
            cr.LineTo(59.078125, 71.828125);
            cr.LineTo(59.078125, 96.828125);
            cr.LineTo(34.078125, 96.828125);
            cr.ClosePath();
            cr.MoveTo(34.078125, 71.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(71.578125, 71.828125);
            cr.LineTo(96.578125, 71.828125);
            cr.LineTo(96.578125, 96.828125);
            cr.LineTo(71.578125, 96.828125);
            cr.ClosePath();
            cr.MoveTo(71.578125, 71.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.FillRule = FillRule.Winding;
            cr.FillPreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(71.578125, 71.828125);
            cr.LineTo(96.578125, 71.828125);
            cr.LineTo(96.578125, 96.828125);
            cr.LineTo(71.578125, 96.828125);
            cr.ClosePath();
            cr.MoveTo(71.578125, 71.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Restore();
        }

        public void Drawremove4_svg(Context cr, int x, int y, float width, float height, double[] rgba)
        {
            Pattern pattern = null;
            Matrix matrix = cr.Matrix;
            cr.Save();
            float num = 129f;
            float num2 = 129f;
            float num3 = Math.Min(width / num, height / num2);
            matrix.Translate((float)x + Math.Max(0f, (width - num * num3) / 2f), (float)y + Math.Max(0f, (height - num2 * num3) / 2f));
            matrix.Scale(num3, num3);
            cr.Matrix = matrix;
            cr.Operator = Operator.Over;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(34.078125, 33.828125);
            cr.LineTo(59.078125, 33.828125);
            cr.LineTo(59.078125, 58.828125);
            cr.LineTo(34.078125, 58.828125);
            cr.ClosePath();
            cr.MoveTo(34.078125, 33.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.FillRule = FillRule.Winding;
            cr.FillPreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(34.078125, 33.828125);
            cr.LineTo(59.078125, 33.828125);
            cr.LineTo(59.078125, 58.828125);
            cr.LineTo(34.078125, 58.828125);
            cr.ClosePath();
            cr.MoveTo(34.078125, 33.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(71.578125, 33.828125);
            cr.LineTo(96.578125, 33.828125);
            cr.LineTo(96.578125, 58.828125);
            cr.LineTo(71.578125, 58.828125);
            cr.ClosePath();
            cr.MoveTo(71.578125, 33.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.FillRule = FillRule.Winding;
            cr.FillPreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(71.578125, 33.828125);
            cr.LineTo(96.578125, 33.828125);
            cr.LineTo(96.578125, 58.828125);
            cr.LineTo(71.578125, 58.828125);
            cr.ClosePath();
            cr.MoveTo(71.578125, 33.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(34.078125, 71.828125);
            cr.LineTo(59.078125, 71.828125);
            cr.LineTo(59.078125, 96.828125);
            cr.LineTo(34.078125, 96.828125);
            cr.ClosePath();
            cr.MoveTo(34.078125, 71.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.FillRule = FillRule.Winding;
            cr.FillPreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(34.078125, 71.828125);
            cr.LineTo(59.078125, 71.828125);
            cr.LineTo(59.078125, 96.828125);
            cr.LineTo(34.078125, 96.828125);
            cr.ClosePath();
            cr.MoveTo(34.078125, 71.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(71.578125, 71.828125);
            cr.LineTo(96.578125, 71.828125);
            cr.LineTo(96.578125, 96.828125);
            cr.LineTo(71.578125, 96.828125);
            cr.ClosePath();
            cr.MoveTo(71.578125, 71.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.FillRule = FillRule.Winding;
            cr.FillPreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(71.578125, 71.828125);
            cr.LineTo(96.578125, 71.828125);
            cr.LineTo(96.578125, 96.828125);
            cr.LineTo(71.578125, 96.828125);
            cr.ClosePath();
            cr.MoveTo(71.578125, 71.828125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Operator = Operator.Over;
            cr.LineWidth = 8.0;
            cr.MiterLimit = 10.0;
            cr.LineCap = LineCap.Butt;
            cr.LineJoin = LineJoin.Miter;
            pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
            cr.SetSource(pattern);
            cr.NewPath();
            cr.MoveTo(108.828125, 21.828125);
            cr.LineTo(19.328125, 111.328125);
            cr.Tolerance = 0.1;
            cr.Antialias = Antialias.Default;
            cr.StrokePreserve();
            pattern?.Dispose();
            cr.Restore();
        }

        #endregion svg
    }
}
