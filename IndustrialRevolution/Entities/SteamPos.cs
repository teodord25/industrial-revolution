using Vintagestory.API.MathTools;

using System.Linq;

namespace IndustrialRevolution.Entities;
public class SteamPos : BlockPos
{
    public bool isFullBlock;

    // NOTE: this grid represents which voxels
    // of this block are occupied by steam
    public bool[,,]? steamGrid;

    public int? countOccupied()
    {
        if (steamGrid == null)
        {
            return null;
        }

        return steamGrid.Cast<bool>().Count(x => x == true);
    }

    private SteamPos(int x, int y, int z, bool isFullBlock, bool[,,]? steamGrid)
        : base(x, y, z)
    {
        this.isFullBlock = isFullBlock;
        this.steamGrid = steamGrid;
    }

    public static SteamPos SolidFromBlockPos(BlockPos pos)
    {
        return new SteamPos(pos.X, pos.Y, pos.Z, true, null);
    }

    public static SteamPos ChsldFromBlockPos(BlockPos pos, bool[,,] steamGrid)
    {
        return new SteamPos(pos.X, pos.Y, pos.Z, false, steamGrid);
    }

    public static SteamPos SolidFromXYZ(int x, int y, int z)
    {
        return new SteamPos(x, y, z, true, null);
    }

    public static SteamPos ChsldFromXYZ(int x, int y, int z, bool[,,] steamGrid)
    {
        return new SteamPos(x, y, z, false, steamGrid);
    }

    public void SetGrid(bool[,,] steamGrid)
    {
        this.steamGrid = steamGrid;
    }
}
