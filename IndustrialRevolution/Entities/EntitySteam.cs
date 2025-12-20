using Vintagestory.API.Common;
using System.Linq;
using Vintagestory.API.Util;
using Vintagestory.API.MathTools;
using IndustrialRevolution.util;

using System.Collections.Generic;

namespace IndustrialRevolution.Entities;

internal partial class EntitySteam : EntityAgent
{
    private ModLogger? log = IndustrialRevolutionModSystem.Logger;
    private HashSet<BlockPos> occupiedVoxels = new HashSet<BlockPos>();
    private int maxVolume = 100; // Limit expansion

    public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode)
    {
        if (mode != EnumInteractMode.Interact) return;

        ExpandSteam();

        this.MarkShapeModified();

        // log?.Debug("-> expanded the steam");
    }

    public void ExpandSteamOnce()
    {
        log?.Debug("expanding steam");
        occupiedVoxels.Clear();
        BlockPos startPos = Pos.AsBlockPos;

        Queue<BlockPos> toCheck = new Queue<BlockPos>();
        HashSet<BlockPos> visited = new HashSet<BlockPos>();

        toCheck.Enqueue(startPos);
        visited.Add(startPos);

        while (visited.Count < 1)
        {
            BlockPos pos = toCheck.Dequeue();
            Block block = World.BlockAccessor.GetBlock(pos);

            if (block.Id == 0 || block.IsLiquid() || !block.SideIsSolid(null, 0))
            {
                occupiedVoxels.Add(pos.Copy());

                foreach (BlockFacing facing in BlockFacing.ALLFACES)
                {
                    BlockPos neighbor = pos.AddCopy(facing);

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        toCheck.Enqueue(neighbor);
                    }
                }
            }
        }

        int[] coords = occupiedVoxels
            .SelectMany(pos => new[] { pos.X, pos.Y, pos.Z })
            .ToArray();

        byte[] voxels = SerializerUtil.Serialize(coords);

        WatchedAttributes.SetBytes("steam-occupied", voxels);
        WatchedAttributes.MarkPathDirty("steam-occupied");

        WatchedAttributes.SetBool("steam-touched", true);
        WatchedAttributes.MarkPathDirty("steam-touched");

        WatchedAttributes.SetInt("steamVolume", occupiedVoxels.Count);
        WatchedAttributes.MarkPathDirty("steamVolume");
    }

    public void ExpandSteam()
    {
        occupiedVoxels.Clear();
        BlockPos startPos = Pos.AsBlockPos;

        Queue<BlockPos> toCheck = new Queue<BlockPos>();
        HashSet<BlockPos> visited = new HashSet<BlockPos>();

        toCheck.Enqueue(startPos);
        visited.Add(startPos);

        while (toCheck.Count > 0 && occupiedVoxels.Count < maxVolume)
        {
            BlockPos pos = toCheck.Dequeue();
            Block block = World.BlockAccessor.GetBlock(pos);

            if (block.Id == 0 || block.IsLiquid() || !block.SideIsSolid(null, 0))
            {
                // log?.Debug("spot is available");

                occupiedVoxels.Add(pos.Copy());

                // log?.Debug("adding pos occupied:" + pos.ToString());

                foreach (BlockFacing facing in BlockFacing.ALLFACES)
                {
                    // log?.Debug("checking neighbours");
                    BlockPos neighbor = pos.AddCopy(facing);
                    // log?.Debug("checking neighbour:" + neighbor.ToString());

                    if (!visited.Contains(neighbor))
                    {
                        // log?.Debug("found free neightbour:" + neighbor.ToString());
                        visited.Add(neighbor);
                        toCheck.Enqueue(neighbor);
                    }
                }
            }
        }

        int[] coords = occupiedVoxels
            .SelectMany(pos => new[] { pos.X, pos.Y, pos.Z })
            .ToArray();

        byte[] voxels = SerializerUtil.Serialize(coords);

        // log?.Debug("expanded steam: " + voxels.Count());

        WatchedAttributes.SetBytes("steam-occupied", voxels);
        WatchedAttributes.MarkPathDirty("steam-occupied");

        WatchedAttributes.SetBool("steam-touched", true);
        WatchedAttributes.MarkPathDirty("steam-touched");

        WatchedAttributes.SetInt("steamVolume", occupiedVoxels.Count);
        WatchedAttributes.MarkPathDirty("steamVolume");
    }
}
