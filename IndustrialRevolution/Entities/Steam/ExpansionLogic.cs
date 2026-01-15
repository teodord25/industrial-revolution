using Vintagestory.API.Common;
using System.Linq;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

using System.Collections.Generic;
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

    private List<(int x, int y, int z)> HolesInFace(
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

        return holes;
    }

    // TODO: simplify
    public void ExpandSteam()
    {
        // TODO: check if root is fullblock
        // TODO: for now allow only 1x1x1 full water block, but I could maybe
        // later add some kind of mechanic where you could place water into a
        // chiseled block and just do a "where would falling water collect" and
        // fill the containers with fake water and set those as the steam source

        (int x, int y, int z) rootPos = (
            this.Pos.AsBlockPos.X,
            this.Pos.AsBlockPos.Y,
            this.Pos.AsBlockPos.Z
        );

        var root = new SteamPos
        { X = rootPos.x, Y = rootPos.y, Z = rootPos.z, IsFullBlock = true };

        if (this.occupied.Count == 0) this.occupied.Add(root);
        if (this.toCheck.Count == 0) this.toCheck.Enqueue(rootPos);

        while (
            this.occupied.Count < this.maxVol?.AsBlocks() &&
            this.toCheck.Count > 0
        )
        {
            var curr = this.toCheck.Dequeue();

            foreach (
                (int x, int y, int z) neighPos in this.GetNeighbors(curr)
            )
            {
                SteamPos neighSteam = SteamPosFactory.SolidFromTuple(neighPos);

                if (this.occupied.Contains(neighSteam)) continue;

                BlockPos blkPos = new BlockPos(
                    neighPos.x, neighPos.y, neighPos.z
                );
                Block neighBlock = World
                    .BlockAccessor
                    .GetBlock(blkPos);

                BlockEntity neighBE = this
                    .Api
                    .World
                    .BlockAccessor
                    .GetBlockEntity(blkPos);

                if (neighBE is BlockEntityMicroBlock beMicroBlock)
                {
                    log?.Debug("is be");
                    // if chiseled, add chiseled
                    var voxelGrid = GetVoxelGrid(beMicroBlock);

                    List<(int x, int y, int z)> holes = HolesInFace(
                        voxelGrid, curr, neighPos
                    );

                    if (holes.Count == 0) continue;

                    bool[,,] steamGrid = this.ExpandChiseled(voxelGrid, holes);
                    neighSteam = neighSteam with
                    { IsFullBlock = false, SteamGrid = steamGrid };
                }
                else if (neighBlock.Id != 0)
                {
                    log?.Debug("is not be, and not air");
                    continue;
                }

                log?.Debug($"adding {neighSteam.ToLocalCoords()} crazy style");

                this.occupied.Add(neighSteam);
                this.toCheck.Enqueue(neighPos);

                // TODO: keep track of these non air blocks for like
                // container detection. Will knowing the mesh of the
                // container be enough to allow for pistons and so on?
                // (changing shapes)
            }
        }

        int fullBlocks = this.occupied.Where(o => o.IsFullBlock).Count();
        int chiseledBlks = this.occupied.Where(o => !o.IsFullBlock).Count();

        SteamVolume? steamVol = SteamVolume.FromBlocks(fullBlocks);
        if (steamVol == null)
            log?.Error("computing full blocks volume went wrong");

        SteamVolume chiseledVol = SteamVolume.FromVoxels(
            this.occupied.Where(o => !o.IsFullBlock)
            .Select(o => o.CountOccupied())
            .Sum()
        );

        steamVol?.AddVoxels(chiseledVol.AsVoxels());

        WatchedAttributes.SetInt("steamVolume", this.occupied.Count);
        WatchedAttributes.MarkPathDirty("steamVolume");

        List<SteamPos> occupiedList = this.occupied.ToList();

        (byte[] steamOccupied, byte[] chiselOccupied) =
            SteamSerializer.Serialize(occupiedList);

        WatchedAttributes.SetBytes("steamOccupied", steamOccupied);
        WatchedAttributes.MarkPathDirty("steamOccupied");

        WatchedAttributes.SetBytes("chiseledSteam", chiselOccupied);
        WatchedAttributes.MarkPathDirty("chiseledSteam");

        int currentVersion = WatchedAttributes.GetInt("steamShapeVersion", 0);
        WatchedAttributes.SetInt("steamShapeVersion", currentVersion + 1);
        WatchedAttributes.MarkPathDirty("steamShapeVersion");
    }
}
