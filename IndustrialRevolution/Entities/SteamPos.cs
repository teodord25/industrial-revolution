using Vintagestory.API.MathTools;

using System.Linq;

namespace IndustrialRevolution.Entities;
public class SteamPos : BlockPos
{
    public bool isFullBlock;

    public byte[,,]? grid;

    public int? countSolid()
    {
        if (grid == null)
        {
            return null;
        }

        return grid.Cast<byte>().Count(x => x != 0);
    }

    private SteamPos(int x, int y, int z, bool isFullBlock, byte[,,]? grid)
        : base(x, y, z)
    {
        this.isFullBlock = isFullBlock;
        this.grid = grid;
    }

    public static SteamPos SolidFromBlockPos(BlockPos pos)
    {
        return new SteamPos(pos.X, pos.Y, pos.Z, true, null);
    }

    public static SteamPos ChsldFromBlockPos(BlockPos pos, byte[,,] grid)
    {
        return new SteamPos(pos.X, pos.Y, pos.Z, false, grid);
    }

    public static SteamPos SolidFromXYZ(int x, int y, int z)
    {
        return new SteamPos(x, y, z, true, null);
    }

    public static SteamPos ChsldFromXYZ(int x, int y, int z, byte[,,] grid)
    {
        return new SteamPos(x, y, z, false, grid);
    }

    public void SetGrid(byte[,,] grid)
    {
        this.grid = grid;
    }
}
