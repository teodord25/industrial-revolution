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

    // TODO: simplify
    public void ExpandSteam()
    {
        // TODO: check if root is fullblock
        // TODO: for now allow only 1x1x1 full water block, but I could maybe
        // later add some kind of mechanic where you could place water into a
        // chiseled block and just do a "where would falling water collect" and
        // fill the containers with fake water and set those as the steam source

        (int x, int y, int z) rootXYZ = (
            this.Pos.AsBlockPos.X,
            this.Pos.AsBlockPos.Y,
            this.Pos.AsBlockPos.Z
        );

        int limit = (int)(this.maxVol?.AsBlocks() ?? 0);

        util.General.BreadthFirstSearch(
            this.occupied, this.toCheck, rootXYZ, limit,
            this.GetNeighbors, SteamPosFactory.SolidFromTuple,
            onVisit: (currXYZ, currPos, neighXYZ, neighPos) =>
            {
                Block neighBlock = World.BlockAccessor
                .GetBlock(
                    new BlockPos(neighXYZ.x, neighXYZ.y, neighXYZ.z)
                );

                BlockEntity neighBE = this.Api.World.BlockAccessor
                .GetBlockEntity(
                    new BlockPos(neighXYZ.x, neighXYZ.y, neighXYZ.z)
                );

                BlockEntity currBE = this.Api.World.BlockAccessor
                .GetBlockEntity(
                    new BlockPos(currXYZ.x, currXYZ.y, currXYZ.z)
                );

                var currBEMB = currBE as BlockEntityMicroBlock;
                var neighBEMB = neighBE as BlockEntityMicroBlock;

                bool neighIsAir = (neighBlock.Id == 0);
                bool neighIsChsld = (neighBEMB != null);
                bool currIsChsld = (currBEMB != null);
                // NOTE: 'solid' here means a solid block (not steam)
                // while 'fullblock' means no chisel state i.e.
                // the entire block is occupied by steam
                // NOTE: 'curr' will always contain steam because its taken
                // from 'occupied', but it might be either:
                //  - a fullblock (entire block is steam)
                //  - a non fullblock (steam occupies part/all of the voids in
                //  the chiseled block)

                // if neigh is solid block
                if (!neighIsChsld && !neighIsAir) return;

                if (neighIsChsld)
                {
                    byte[,,] neighVoxelGrid = GetVoxelGrid(neighBEMB!);
                    (int x, int y, int z)[] neighFaceHoles = HolesInFace(
                        neighVoxelGrid,
                        (currPos.X, currPos.Y, currPos.Z),
                        (neighPos.X, neighPos.Y, neighPos.Z)
                    );

                    // if curr is fullblock and
                    // neigh is chsld but has no face holes
                    if (!currIsChsld && neighFaceHoles.Length == 0) return;

                    if (currIsChsld)
                    {
                        (int x, int y, int z)[] currFaceHoles = HolesInFace(
                            GetVoxelGrid(currBEMB!),
                            (neighPos.X, neighPos.Y, neighPos.Z),
                            (currPos.X, currPos.Y, currPos.Z)
                        );

                        neighFaceHoles = GetConnectingHoles(
                            currPos, neighPos,
                            currFaceHoles.ToArray(), neighFaceHoles.ToArray()
                        );
                    }

                    bool[,,] neighSteamGrid = this.ExpandChiseled(
                        neighVoxelGrid, neighFaceHoles
                    );

                    neighPos = neighPos with
                    { IsFullBlock = false, SteamGrid = neighSteamGrid };

                }
                // if neigh is not chiseled but is air
                // and curr is chiseled
                else if (currIsChsld && neighIsAir)
                {
                    (int x, int y, int z)[] currFaceHoles = HolesInFace(
                        GetVoxelGrid(currBEMB!),
                        (neighPos.X, neighPos.Y, neighPos.Z),
                        (currPos.X, currPos.Y, currPos.Z)
                    );

                    if (currPos.SteamGrid == null) {
                        log?.Warning(
                            $"pos {currPos.ToLocalCoords()}" +
                            " should have grid but doesn't");
                        return;
                    }

                    bool anyHoles = currFaceHoles
                        .Where(h => currPos.SteamGrid[h.x, h.y, h.z])
                        .Any();

                    if (!anyHoles) return;
                }

                this.occupied.Add(neighXYZ, neighPos);
                this.toCheck.Enqueue(neighXYZ);
            }
        );

        // TODO: keep track of these non air blocks for like
        // container detection. Will knowing the mesh of the
        // container be enough to allow for pistons and so on?
        // (changing shapes)

        int fullBlocks = this
            .occupied
            .Where(o => o.Value.IsFullBlock)
            .Count();

        int chiseledBlks = this
            .occupied
            .Where(o => !o.Value.IsFullBlock)
            .Count();

        SteamVolume? steamVol = SteamVolume.FromBlocks(fullBlocks);
        if (steamVol == null)
            log?.Error("computing full blocks volume went wrong");

        SteamVolume chiseledVol = SteamVolume.FromVoxels(
            this.occupied.Where(o => !o.Value.IsFullBlock)
            .Select(o => o.Value.CountOccupied())
            .Sum()
        );

        steamVol?.AddVoxels(chiseledVol.AsVoxels());

        WatchedAttributes.SetInt("steamVolume", this.occupied.Count);
        WatchedAttributes.MarkPathDirty("steamVolume");

        List<SteamPos> occupiedList = this.occupied.Values.ToList();

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
