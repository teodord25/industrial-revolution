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

    private bool PassableFace(byte[,,] voxelGrid, BlockPos from, BlockPos to)
    {
        BlockFacing face = BlockFacing.FromVector(
            to.X - from.X,
            to.Y - from.Y,
            to.Z - from.Z
        ).Opposite;

        (EnumAxis axis, int const_val) = GetCoordinateStartForFace(face);

        List<int[]> holes = new List<int[]>();

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                byte voxel = 0;
                int[] pos = { };

                if (axis == EnumAxis.X) pos = [const_val, i, j];
                if (axis == EnumAxis.Y) pos = [i, const_val, j];
                if (axis == EnumAxis.Z) pos = [i, j, const_val];

                voxel = voxelGrid[pos[0], pos[1], pos[2]];

                if (voxel == 0) holes.Add(pos);
            }
        }

        if (holes.Count > 0) return true;

        return false;
    }

    // TODO: simplify
    public void ExpandSteam()
    {
        // TODO: check if root is fullblock
        // TODO: for now allow only 1x1x1 full water block, but I could maybe
        // later add some kind of mechanic where you could place water into a
        // chiseled block and just do a "where would falling water collect" and
        // fill the containers with fake water and set those as the steam source
        var root = this.Pos.AsBlockPos;

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

                BlockEntity neighBE = this
                    .Api
                    .World
                    .BlockAccessor
                    .GetBlockEntity(neigh);

                if (neighBE is BlockEntityMicroBlock beMicroBlock)
                {
                    var voxelGrid = GetVoxelGrid(beMicroBlock);
                    if (this.PassableFace(voxelGrid, curr, neigh))
                    { log?.Debug("OH YEAH"); }
                    else { log?.Debug("bruhhhhhh"); }

                    List<uint> cuboids = beMicroBlock.VoxelCuboids;
                    var blockIds = beMicroBlock.BlockIds;
                    // this.ExpandThroughChiseled(beMicroBlock);

                    this.chiseled.Add(neigh);
                }
                else
                { // if not blockentity and not air; skip
                    if (neighBlock.Id != 0) continue;
                }

                // TODO: keep track of these non air blocks for like
                // container detection. Will knowing the mesh of the
                // container be enough to allow for pistons and so on?
                // (changing shapes)

                this.occupied.Add(neigh);
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
