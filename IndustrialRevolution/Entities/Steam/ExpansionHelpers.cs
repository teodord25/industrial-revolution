using Vintagestory.API.Common;

using System.Collections.Generic;
using Vintagestory.API.MathTools;

using System.Linq;
using System;

namespace IndustrialRevolution.Entities.Steam;

internal partial class EntitySteam : EntityAgent
{
    private (int x, int y, int z)[] GetNeighbors((int x, int y, int z) pos)
    {
        (int x, int y, int z)[] neighbors = new (int x, int y, int z)[6];

        var (x, y, z) = pos;

        neighbors[0] = (x + 1, y, z);
        neighbors[1] = (x - 1, y, z);
        neighbors[2] = (x, y + 1, z);
        neighbors[3] = (x, y - 1, z);
        neighbors[4] = (x, y, z + 1);
        neighbors[5] = (x, y, z - 1);

        return neighbors;
    }

    private (int x, int y, int z)[] NeighborVoxels((int x, int y, int z) pos)
    {
        (int x, int y, int z)[] neighbors = this.GetNeighbors(pos);

        return neighbors
            .Where(n => n.x >= 0 && n.x < 16)
            .Where(n => n.y >= 0 && n.y < 16)
            .Where(n => n.z >= 0 && n.z < 16)
            .ToArray();
    }

    private (EnumAxis, int) GetCoordinateStartForFace(BlockFacing face)
    {
        if (face == BlockFacing.NORTH) return (EnumAxis.Z, 0);
        if (face == BlockFacing.SOUTH) return (EnumAxis.Z, 15);
        if (face == BlockFacing.WEST) return (EnumAxis.X, 0);
        if (face == BlockFacing.EAST) return (EnumAxis.X, 15);
        if (face == BlockFacing.DOWN) return (EnumAxis.Y, 0);
        if (face == BlockFacing.UP) return (EnumAxis.Y, 15);

        return (EnumAxis.Y, 15);
    }

    private (int x, int y, int z)[] HolesInFace(
        byte[,,] voxelGrid,
        (int x, int y, int z) from,
        (int x, int y, int z) to
    )
    {
        BlockFacing face = BlockFacing.FromVector(
            to.x - from.x,
            to.y - from.y,
            to.z - from.z
        ).Opposite;

        (EnumAxis axis, int const_val) = GetCoordinateStartForFace(face);

        List<(int x, int y, int z)> holes = new List<(int x, int y, int z)>();

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                byte voxel = 0;
                var pos = axis switch
                {
                    EnumAxis.X => (const_val, i, j),
                    EnumAxis.Y => (i, const_val, j),
                    EnumAxis.Z => (i, j, const_val),
                    _ => throw new ArgumentException($"Invalid axis: {axis}")
                };
                voxel = voxelGrid[pos.Item1, pos.Item2, pos.Item3];

                if (voxel == 0) holes.Add(pos);
            }
        }

        return holes.ToArray();
    }

    public (int x, int y, int z)[] GetConnectingHoles(
        SteamPos currPos, SteamPos neighPos,
        (int x, int y, int z)[] currFaceHoles,
        (int x, int y, int z)[] neighFaceHoles
    )
    {
        if (currFaceHoles.Length == 0) return [];
        if (currPos.SteamGrid == null)
        {
            log?.Warning(
                $"Steampos {currPos.ToLocalCoords()}" +
                "has no grid"
            );
            return [];
        }

        // 1. if steam occupies any of the holes
        // on the connecting face of curr
        HashSet<(int x, int y, int z)> filledHoles =
            currFaceHoles
            .Where(h => currPos.SteamGrid[h.x, h.y, h.z])
            .ToHashSet();

        // 2. and at least one hole on the connecting
        // face of curr lines up with a hole on the
        // connecting face of neigh
        (int x, int y, int z)[] matchingHoles = neighFaceHoles
            .Where(h =>
            {

                BlockFacing face = BlockFacing.FromVector(
                    neighPos.X - currPos.X,
                    neighPos.Y - currPos.Y,
                    neighPos.Z - currPos.Z
                );

                (EnumAxis axis, int const_val) = GetCoordinateStartForFace(face);

                var pos = axis switch
                {
                    EnumAxis.X => (const_val, h.y, h.z),
                    EnumAxis.Y => (h.x, const_val, h.z),
                    EnumAxis.Z => (h.x, h.y, const_val),
                    _ => throw new ArgumentException($"Invalid axis: {axis}")
                };

                return filledHoles.Contains(pos);
            })
            .ToArray();

        if (matchingHoles.Length == 0) return [];

        return matchingHoles.ToArray();
    }
}
