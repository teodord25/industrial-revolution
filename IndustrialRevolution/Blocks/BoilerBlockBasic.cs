using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Config;

namespace IndustrialRevolution
{
    public class BlockBoilerBasic : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.Side == EnumAppSide.Server)
            {

                IServerPlayer serverPlayer = byPlayer as IServerPlayer;

                serverPlayer?.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    "You interacted with the boiler!",
                    EnumChatType.Notification
                );

                world.Logger.Notification("hello");

                Block block = world.BlockAccessor.GetBlock(blockSel.Position);

                // Remove the block
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                world.BlockAccessor.MarkBlockDirty(blockSel.Position);

                // Try to get the falling block entity type
                EntityProperties type = world.GetEntityType(new AssetLocation("game", "goat-muskox-male-adult"));

                world.Logger.Notification($"EntityProperties type is: {(type == null ? "NULL" : "NOT NULL")}");


                if (type == null)
                {
                    world.Logger.Error("Could not find fallingblock entity type!");
                    world.SpawnItemEntity(new ItemStack(block), blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
                    return true;
                }

                world.Logger.Notification("About to create entity...");
                Entity entity = world.ClassRegistry.CreateEntity(type);
                world.Logger.Notification($"Entity created: {(entity == null ? "NULL" : entity.GetType().Name)}");

                if (entity == null)
                {
                    world.Logger.Error("Failed to create entity!");
                    world.SpawnItemEntity(new ItemStack(block), blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
                    return true;
                }

                // Position the entity
                entity.ServerPos.SetPos(blockSel.Position.ToVec3d().Add(0.5, 0, 0.5));
                entity.Pos.SetFrom(entity.ServerPos);

                world.Logger.Notification($"Setting blockId to: {block.Id}");
                entity.WatchedAttributes.SetInt("blockId", block.Id);

                world.Logger.Notification("About to spawn entity...");
                world.SpawnEntity(entity);

                world.Logger.Notification($"Successfully spawned falling block for blockId {block.Id}");
            }

            return true;
        }
    }
}
