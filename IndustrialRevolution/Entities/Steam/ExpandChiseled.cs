using Vintagestory.API.Common;
using Vintagestory.GameContent;

using System.Linq;

using System.Collections.Generic;
using System;

namespace IndustrialRevolution.Entities.Steam;

// TODO: add some kind of collision detection or use repulse agent or something
// to decide when a valve or something can cut off part of the steam and create
// a second steam entity
internal partial class EntitySteam : EntityAgent
{
    private bool[,,] ExpandChiseled(
        byte[,,] voxelGrid, (int x, int y, int z)[] holes
    )
    {
        // TODO: maybe rework this to not use the hashset at all
        int freeVoxelsInBlock = voxelGrid
            .Cast<byte>()
            .Where(v => v == 0)
            .Count();

        HashSet<(int x, int y, int z)> occupiedVoxels =
            new HashSet<(int x, int y, int z)>();

        Queue<(int x, int y, int z)> to_checkVoxels =
            new Queue<(int, int, int)>();

        bool[,,] steamGrid = new bool[16, 16, 16];

        foreach ((int x, int y, int z) hole in holes)
        {
            if (!occupiedVoxels.Contains(hole))
            {
                occupiedVoxels.Add(hole);
                steamGrid[hole.x, hole.y, hole.z] = true;
            }
            if (!to_checkVoxels.Contains(hole)) to_checkVoxels.Enqueue(hole);
        }

        while (
            occupiedVoxels.Count < freeVoxelsInBlock &&
            to_checkVoxels.Count > 0
        )
        {
            var curr = to_checkVoxels.Dequeue();

            foreach ((int x, int y, int z) neigh in this.NeighborVoxels(curr))
            {
                if (neigh.x < 0 || neigh.x >= 16 ||
                    neigh.y < 0 || neigh.y >= 16 ||
                    neigh.z < 0 || neigh.z >= 16)
                {
                    continue;  // skip out of bound neighbours
                }

                if (occupiedVoxels.Contains(neigh)) continue;

                byte neighMatId = voxelGrid[neigh.x, neigh.y, neigh.z];

                // if not air skip
                if (neighMatId != 0) continue;

                occupiedVoxels.Add(neigh);
                to_checkVoxels.Enqueue(neigh);

                steamGrid[neigh.x, neigh.y, neigh.z] = true;
            }
        }

        log?.Debug($"steamgrid: {steamGrid.Cast<bool>().Where(v => v == true).Count()}");

        int currentVersion = WatchedAttributes.GetInt("steamShapeVersion", 0);
        WatchedAttributes.SetInt("steamShapeVersion", currentVersion + 1);
        WatchedAttributes.MarkPathDirty("steamShapeVersion");

        return steamGrid;
    }

    private byte[,,] GetVoxelGrid(BlockEntityMicroBlock beMicroBlock)
    {
        byte[,,] voxelGrid = new byte[16, 16, 16];

        List<uint> cuboids = beMicroBlock.VoxelCuboids;

        foreach (uint cuboid in cuboids)
        {
            var (x1, y1, z1, x2, y2, z2, matId) = DecodeVoxel(cuboid);

            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    for (int z = z1; z <= z2; z++)
                    {
                        // NOTE: 0 is reserved for "empty voxel"
                        // so we offset matId by 1
                        voxelGrid[x, y, z] = (byte)(matId + 1);
                    }
                }
            }
        }

        return voxelGrid;
    }

    private (
        int x1, int y1, int z1,
        int x2, int y2, int z2,
        byte materialIndex
    ) DecodeVoxel(uint encoded)
    {
        // unpack nibbles into coords
        int x1 = (int)((encoded >> 0) & 0xF);
        int y1 = (int)((encoded >> 4) & 0xF);
        int z1 = (int)((encoded >> 8) & 0xF);
        int x2 = (int)((encoded >> 12) & 0xF);
        int y2 = (int)((encoded >> 16) & 0xF);
        int z2 = (int)((encoded >> 20) & 0xF);

        // remaining bits represent the materialindex
        byte materialIndex = (byte)((encoded >> 24) & 0xFF);

        return (x1, y1, z1, x2, y2, z2, materialIndex);
    }
}
