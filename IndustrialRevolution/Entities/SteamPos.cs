using Vintagestory.API.MathTools;

namespace IndustrialRevolution.Entities;
public class SteamPos : BlockPos
{
    public bool isFullBlock;

    private SteamPos(bool isFullBlock, int x, int y, int z) : base(x, y, z)
    {
        this.isFullBlock = isFullBlock;
    }

    public static SteamPos FromBlockPos(bool isFullBlock, BlockPos pos)
    {
        return new SteamPos(isFullBlock, pos.X, pos.Y, pos.Z);
    }
}
