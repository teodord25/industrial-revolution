using Vintagestory.API.Common;
using Vintagestory.GameContent;

using System.Collections.Generic;

namespace IndustrialRevolution.Entities;

// TODO: add some kind of collision detection or use repulse agent or something
// to decide when a valve or something can cut off part of the steam and create
// a second steam entity
internal partial class EntitySteam : EntityAgent
{
    private byte[,,] GetVoxelGrid(BlockEntityMicroBlock beMicroBlock)
    {
        byte[,,] voxelGrid = new byte[16, 16, 16];

        List<uint> cuboids = beMicroBlock.VoxelCuboids;
        var blkIds = beMicroBlock.BlockIds;

        foreach (uint cuboid in cuboids)
        {
            var (x1, y1, z1, x2, y2, z2, matId) = DecodeVoxel(cuboid, blkIds);

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
    ) DecodeVoxel(uint encoded, int[] blockIds)
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
