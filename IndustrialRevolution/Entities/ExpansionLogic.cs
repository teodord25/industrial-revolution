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

    private bool PassableFace(byte[,,] voxelGrid, BlockPos from, BlockPos to)
    {
        int x = 0, y = 0, z = 0;
        bool x_face = false, y_face = false, z_face = false;

        BlockFacing face = BlockFacing.FromVector(
            to.X - from.X,
            to.Y - from.Y,
            to.Z - from.Z
        ).Opposite;

        if (face == BlockFacing.NORTH) { z_face = true; z = 0; }
        if (face == BlockFacing.SOUTH) { z_face = true; z = 15; }
        if (face == BlockFacing.WEST) { x_face = true; x = 0; }
        if (face == BlockFacing.EAST) { x_face = true; x = 15; }
        if (face == BlockFacing.DOWN) { y_face = true; y = 0; }
        if (face == BlockFacing.UP) { y_face = true; y = 15; }

        List<int[]> holes = new List<int[]>();

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                byte voxel = 0;
                int[] pos = { };

                if (x_face) pos = [x, i, j];
                if (y_face) pos = [i, y, j];
                if (z_face) pos = [i, j, z];

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
                    var voxelGrid = GetVoxelGrid(beMicroBlock);
                    if (this.PassableFace(voxelGrid, curr, neigh))
                    { log?.Debug("OH YEAH"); }
                    else { log?.Debug("bruhhhhhh"); }

                    List<uint> cuboids = beMicroBlock.VoxelCuboids;
                    var blockIds = beMicroBlock.BlockIds;
                    // this.ExpandThroughChiseled(beMicroBlock);

                    steampos = SteamPos.FromBlockPos(false, neigh);
                }
                else
                {
                    // if not blockentity and not air; skip
                    if (neighBlock.Id != 0) continue;
                }

                // TODO: keep track of these non air blocks for like
                // container detection. Will knowing the mesh of the
                // container be enough to allow for pistons and so on?
                // (changing shapes)

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
