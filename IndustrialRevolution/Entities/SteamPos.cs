using Vintagestory.API.MathTools;

using System.Linq;

namespace IndustrialRevolution.Entities;
public class SteamPos : BlockPos
{
    public bool isFullBlock;

    public byte[,,]? grid;

    public int? countSolid() {
        if (grid == null) {
            return null;
        }

        return grid.Cast<byte>().Count(x => x != 0);
    }

    private SteamPos(bool isFullBlock, int x, int y, int z) : base(x, y, z)
    {
        this.isFullBlock = isFullBlock;
        this.grid = null;
    }

    public static SteamPos FromBlockPos(bool isFullBlock, BlockPos pos)
    {
        return new SteamPos(isFullBlock, pos.X, pos.Y, pos.Z);
    }
}
