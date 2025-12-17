using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace IndustrialRevolution.Blocks;

internal class BlockBoiler : Block
{
    public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
    {
        if (isImpact && facing.IsVertical)
        {
            entity.Pos.Motion.Y *= -0.8f;
        }
    }

    public override bool OnBlockInteractStart(
            IWorldAccessor world,
            IPlayer byPlayer,
            BlockSelection blockSel
    )
    {
        if (world.Side == EnumAppSide.Server)
        {
            IServerPlayer serverPlayer = byPlayer as IServerPlayer;

            serverPlayer?.SendMessage(
                GlobalConstants.GeneralChatGroup,
                "You interacted with the boiler!",
                EnumChatType.Notification
            );

            Block block = world.BlockAccessor.GetBlock(blockSel.Position);

            world.BlockAccessor.SetBlock(0, blockSel.Position);
            world.BlockAccessor.MarkBlockDirty(blockSel.Position);

            EntityProperties chickenProp = world.GetEntityType(new AssetLocation("game:goat-muskox-adult-male"));
            IndustrialRevolutionModSystem.Logger?.Debug("chicken is: " + chickenProp);

            Entity chicken = world.ClassRegistry.CreateEntity(chickenProp);

            chicken.ServerPos.X = blockSel.Position.X;
            chicken.ServerPos.Y = blockSel.Position.Y + 1;
            chicken.ServerPos.Z = blockSel.Position.Z;
            chicken.Pos.SetPos(chicken.ServerPos);

            world.SpawnEntity(chicken);
        }

        return true;
    }
}
