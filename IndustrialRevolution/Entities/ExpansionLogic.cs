using Vintagestory.API.Common;
using System.Linq;
using Vintagestory.API.Util;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

using System.Collections.Generic;
using System;

namespace IndustrialRevolution.Entities;

internal partial class EntitySteam : EntityAgent
{
    private BlockPos[] NeighborPositions(BlockPos pos)
    {
        BlockPos[] neighbors = new BlockPos[6];

        int n = 0;

        // TODO: do LINQ here maybe
        foreach (BlockFacing facing in BlockFacing.ALLFACES)
        {
            BlockPos neighbor = pos.AddCopy(facing);

            neighbors[n] = neighbor;
            n++;
        }

        return neighbors;
    }

    private void ExpandThroughChiseled(BlockEntityMicroBlock BE)
    {
        // TODO:
    }

    private void DecodeVoxel(uint encoded, int[] blockIds)
    {
        // TODO:
    }

    // TODO: simplify
    public void ExpandSteam()
    {
        // TODO: check if root is fullblock
        var root = SteamPos.FromBlockPos(true, this.Pos.AsBlockPos);

        if (this.occupied.Count == 0) this.occupied.Add(root);
        if (this.to_check.Count == 0) this.to_check.Enqueue(root);

        while (
            occupied.Count < this.maxVol?.AsBlocks() &&
            this.to_check.Count() > 0
        )
        {
            var curr = this.to_check.Dequeue();

            foreach (BlockPos neigh in this.NeighborPositions(curr))
            {
                Block neighBlock = World.BlockAccessor.GetBlock(neigh);

                if (this.occupied.Contains(neigh)) continue;

                SteamPos steampos = SteamPos.FromBlockPos(true, neigh);

                BlockEntity neighBE = this
                    .Api
                    .World
                    .BlockAccessor
                    .GetBlockEntity(neigh);

                if (neighBE is BlockEntityMicroBlock beMicroBlock)
                {
                    List<uint> cuboids = beMicroBlock.VoxelCuboids;
                    var blockIds = beMicroBlock.BlockIds;

                    foreach (uint cuboid in cuboids)
                        // log?.Debug(String.Join(", ", DecodeVoxel(cuboid, blockIds)));

                    this.ExpandThroughChiseled(beMicroBlock);

                    steampos = SteamPos.FromBlockPos(false, neigh);
                } else {
                    // if not blockentity and not air; skip
                    if (neighBlock.Id != 0) continue;
                }

                // TODO: keep track of these non air blocks for like
                // container detection. Will knowing the mesh of the
                // container be enough to allow for pistons and so on? (changing shapes)

                this.occupied.Add(steampos);
                this.to_check.Enqueue(neigh);
            }
        }

        int[] coords = this.occupied
            .SelectMany(pos => new[] { pos.X, pos.Y, pos.Z })
            .ToArray();

        byte[] voxels = SerializerUtil.Serialize(coords);

        WatchedAttributes.SetBytes("steamOccupied", voxels);
        WatchedAttributes.MarkPathDirty("steamOccupied");

        WatchedAttributes.SetInt("steamVolume", this.occupied.Count);
        WatchedAttributes.MarkPathDirty("steamVolume");

        int currentVersion = WatchedAttributes.GetInt("steamShapeVersion", 0);
        WatchedAttributes.SetInt("steamShapeVersion", currentVersion + 1);
        WatchedAttributes.MarkPathDirty("steamShapeVersion");
    }
}
