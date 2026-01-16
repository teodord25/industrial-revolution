using System;
using System.Linq;

using Vintagestory.API.MathTools;

namespace IndustrialRevolution.Entities.Steam;

public readonly record struct SteamPos
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Z { get; init; }
    public bool IsFullBlock { get; init; }
    public bool[,,]? SteamGrid { get; init; }

    public BlockPos ToBlockPos() => new BlockPos(X, Y, Z);

    public int CountOccupied()
        => SteamGrid?.Cast<bool>().Count(x => x) ?? 0;

    public bool Equals(SteamPos other)
        => X == other.X && Y == other.Y && Z == other.Z;

    public override int GetHashCode()
        => HashCode.Combine(X, Y, Z);

    public (int x, int y, int z) ToLocalCoords()
        => (
            this.X - 512_000,
            this.Y,
            this.Z - 512_000
        );
}

public static class SteamPosFactory
{
    public static SteamPos SolidFromTuple((int x, int y, int z) pos)
        => new SteamPos
        { X = pos.x, Y = pos.y, Z = pos.z, IsFullBlock = true };

    public static SteamPos ChsldFromTuple((int x, int y, int z) pos)
        => new SteamPos
        { X = pos.x, Y = pos.y, Z = pos.z, IsFullBlock = false };

    // public static SteamPos SolidFromXYZ(int x, int y, int z)
    //     => new SteamPos
    //     { X = x, Y = y, Z = z, IsFullBlock = true };
    //
    // public static SteamPos ChsldFromXYZ(int x, int y, int z, bool[,,] steamGrid)
    //     => new SteamPos
    //     { X = x, Y = y, Z = z, IsFullBlock = false };
    //
    // public static SteamPos SolidFromBlockPos(BlockPos pos)
    //     => new SteamPos
    //     {
    //         X = pos.X,
    //         Y = pos.Y,
    //         Z = pos.Z,
    //         IsFullBlock = true,
    //         SteamGrid = null
    //     };
    //
    // public static SteamPos ChsldFromBlockPos(BlockPos pos, bool[,,] steamGrid)
    //     => new SteamPos
    //     {
    //         X = pos.X,
    //         Y = pos.Y,
    //         Z = pos.Z,
    //         IsFullBlock = false,
    //         SteamGrid = steamGrid
    //     };
}
