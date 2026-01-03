using Vintagestory.API.Common;
using System.Linq;
using Vintagestory.API.Util;
using Vintagestory.API.MathTools;
using System.Collections.Generic;

namespace IndustrialRevolution.Entities;

public class SteamVolume
{
    private long totalVolumeInVoxels;

    private SteamVolume(long voxels) { totalVolumeInVoxels = voxels; }

    public static SteamVolume FromVoxels(long voxels)
    {
        return new SteamVolume(voxels);
    }

    public static SteamVolume? FromBlocks(decimal blocks)
    {
        // if the blocks arent a multiple of 1/(16*16*16) reject
        if (blocks * 4096 % 1 != 0) return null;
        return new SteamVolume((long)(blocks * 4096));
    }

    public long AsVoxels() => this.totalVolumeInVoxels;
    public decimal AsBlocks() => this.totalVolumeInVoxels / 4096;
}

internal partial class EntitySteam : EntityAgent
{
    private HashSet<BlockPos> occupied = new HashSet<BlockPos>();
    private Queue<BlockPos> to_check = new Queue<BlockPos>();

    private SteamVolume totVol = SteamVolume.FromVoxels(0);
    private SteamVolume? maxVol = SteamVolume.FromBlocks(4);

    private int maxVolumeInBlocks = 15;

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

    private bool IsPassable(BlockPos from, BlockPos to)
    {
        if (World.BlockAccessor.GetBlock(to).Id == 0) return true;

        // TODO: keep track of these non air blocks for like
        // container detection. Will knowing the mesh of the
        // container be enough to allow for pistons and so on? (changing shapes)
        // BlockFacing direction = BlockFacing.FromVector(
        //     to.X - from.X,
        //     to.Y - from.Y,
        //     to.Z - from.Z
        // );

        return true;
    }

    public void ExpandSteam()
    {
        var root = this.Pos.AsBlockPos;

        if (this.occupied.Count == 0) this.occupied.Add(root);
        if (this.to_check.Count == 0) this.to_check.Enqueue(root);

        while (
            occupied.Count < this.maxVolumeInBlocks &&
            this.to_check.Count() > 0
        )
        {
            var curr = this.to_check.Dequeue();

            foreach (BlockPos pos in this.NeighborPositions(curr))
            {
                if (this.occupied.Contains(pos)) continue;
                if (!this.IsPassable(curr, pos)) continue;

                // TODO
                // if (this.IsChiseled(pos)) this.ExpandThroughChiseled(pos)

                this.occupied.Add(pos.Copy());

                this.to_check.Enqueue(pos);
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
