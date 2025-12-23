using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
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
    private int maxVolume = 300;

    public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
    {
        base.Initialize(properties, api, InChunkIndex3d);

        if (api.Side == EnumAppSide.Client)
        {
            WatchedAttributes.RegisterModifiedListener("steamShapeVersion", () =>
            {
                log?.Debug("Client detected steamShapeVersion change, marking shape modified");
                this.MarkShapeModified();
            });
        }
    }

    public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode)
    {
        if (mode != EnumInteractMode.Interact) return;

        ExpandSteam();

        this.MarkShapeModified();
    }

    public void ExpandSteamNext(int voxelCount)
    {
        BlockPos startPos = Pos.AsBlockPos;
        Queue<BlockPos> toCheck = new Queue<BlockPos>();
        HashSet<BlockPos> visited = new HashSet<BlockPos>();

        foreach (var occupied in occupiedVoxels)
        {
            visited.Add(occupied);
        }

        foreach (var occupied in occupiedVoxels)
        {
            toCheck.Enqueue(occupied);
        }

        if (occupiedVoxels.Count == 0)
        {
            toCheck.Enqueue(startPos);
            visited.Add(startPos);
        }

        // target = CURRENT + desired increase
        int targetVoxelCount = occupiedVoxels.Count + voxelCount;

        log?.Debug("visited:" + visited.Count);
        while (toCheck.Count > 0 && occupiedVoxels.Count < targetVoxelCount)
        {
            BlockPos pos = toCheck.Dequeue();
            Block block = World.BlockAccessor.GetBlock(pos);

            log?.Debug("checking:" + pos.ToString());

            if (block.Id == 0 || block.IsLiquid() || !block.SideIsSolid(null, 0))
            {
                // Only add if not already occupied
                if (!occupiedVoxels.Contains(pos))
                {
                    occupiedVoxels.Add(pos.Copy());
                }

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

        log?.Debug("voxels: " + voxels.Count());

        WatchedAttributes.SetBytes("steam-occupied", voxels);
        WatchedAttributes.MarkPathDirty("steam-occupied");

        WatchedAttributes.SetBool("steam-touched", true);
        WatchedAttributes.MarkPathDirty("steam-touched");

        WatchedAttributes.SetInt("steamVolume", occupiedVoxels.Count);
        WatchedAttributes.MarkPathDirty("steamVolume");

        log?.Debug("about to mark shape modified");
        this.MarkShapeModified();

        int currentVersion = WatchedAttributes.GetInt("steamShapeVersion", 0);
        log?.Debug("current ver" + currentVersion);

        WatchedAttributes.SetInt("steamShapeVersion", currentVersion + 1);

        log?.Debug("bumped to ver" + WatchedAttributes.GetInt("steamShapeVersion"));
        WatchedAttributes.MarkPathDirty("steamShapeVersion");

        log?.Debug("shape marked as modified");
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
