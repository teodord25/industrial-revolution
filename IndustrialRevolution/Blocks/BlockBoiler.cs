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

            EntityProperties type = world.GetEntityType(new AssetLocation("game", "goat-muskox-male-adult"));

            if (type == null)
            {
                world.Logger.Error("Could not find fallingblock entity type!");
                world.SpawnItemEntity(new ItemStack(block), blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
                return true;
            }

            EntityProperties entityType = world.GetEntityType(new AssetLocation("game:chicken"));

            Entity entity = world.ClassRegistry.CreateEntity(entityType);

            entity.ServerPos.X = 0;
            entity.ServerPos.Y = 0;
            entity.ServerPos.Z = 0;

            entity.Pos.SetPos(entity.ServerPos);

            world.SpawnEntity(entity);
        }

        return true;
    }
}
