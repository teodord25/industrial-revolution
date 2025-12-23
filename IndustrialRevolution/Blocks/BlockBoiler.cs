using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
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
}
