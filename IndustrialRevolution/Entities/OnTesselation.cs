using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using System.Collections.Generic;
using Vintagestory.API.Client;
using System;

namespace IndustrialRevolution.Entities;

internal partial class EntitySteam : EntityAgent
{
    // TODO: only draw edge voxels, somehow, maybe through toCheck or something
    // because drawing in occluded voxels for chiseled blocks could get crazy...
    // TODO: check if steamPos here properly does contains by val or not
    public HashSet<SteamPos> GetOccupied()
    {
        byte[] posData = WatchedAttributes.GetBytes("steamOccupied");
        if (posData == null) return new HashSet<SteamPos>();

        int[] coords = SerializerUtil.Deserialize<int[]>(posData);
        List<SteamPos> positions = new List<SteamPos>();

        for (int i = 0; i < coords.Length; i += 3)
        {
            int x = coords[i];
            int y = coords[i + 1];
            int z = coords[i + 2];

            positions.Add(SteamPos.SolidFromXYZ(x, y, z));
        }

        byte[] chslData = WatchedAttributes.GetBytes("chiseledSteam");
        for (int i = 0; i < chslData.Length; i += 4 + 4096)
        {
            int index = BitConverter.ToInt32(chslData, i);
            bool[,,] steamGrid = new bool[16, 16, 16];
            Buffer.BlockCopy(chslData, i + 4, steamGrid, 0, 4096);

            var pos = positions[index];
            var local = pos.ToLocalPosition(this.Api);

            log?.Debug($"damn: ({index}) {local.X} {local.Z}");

            pos.SetGrid(steamGrid);
            pos.isFullBlock = false;
        }

        var steamPositions = new HashSet<SteamPos>(positions);

        return steamPositions;
    }

    private void bake(
        Shape parentShape,
        Shape childShape,
        string childLocationForLogging,
        string parentLocationForLogging,
        ICoreClientAPI capi
    )
    {
        parentShape.StepParentShape(
            childShape,
            childLocationForLogging,
            parentLocationForLogging,
            capi.Logger,
            (textureCode, textureLoc) =>
            {
                var ctex = new CompositeTexture(textureLoc);
                ctex.Bake(capi.Assets);

                capi.EntityTextureAtlas.GetOrInsertTexture(
                    ctex.Baked.TextureFilenames[0],
                    out int subId,
                    out _
                );

                ctex.Baked.TextureSubId = subId;
                this.Properties.Client.Textures[textureCode] = ctex;
            }
        );
    }

    private void fillVoxels(
        bool[,,] steamGrid,
        SteamPos pos, BlockPos root, Shape voxShape,
        ICoreClientAPI capi, AssetLocation voxShapeLoc,
        ref Shape entityShape, string shapePathForLogging, ref bool shapeIsCloned
    )
    {
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    if (steamGrid[x, y, z] == false) continue;

                    voxShape = voxShape.Clone();

                    int offsetX = (pos.X - root.X) * 16 + x;
                    int offsetY = (pos.Y - root.Y) * 16 + y;
                    int offsetZ = (pos.Z - root.Z) * 16 + z;

                    voxShape.Elements[0].From[0] = offsetX;
                    voxShape.Elements[0].From[1] = offsetY;
                    voxShape.Elements[0].From[2] = offsetZ;

                    voxShape.Elements[0].To[0] = offsetX + 1;
                    voxShape.Elements[0].To[1] = offsetY + 1;
                    voxShape.Elements[0].To[2] = offsetZ + 1;

                    this.bake(
                        entityShape,
                        voxShape,
                        voxShapeLoc.ToString(),
                        shapePathForLogging,
                        capi
                    );
                }
            }
        }
    }

    protected override void OnTesselation(ref Shape entityShape, string shapePathForLogging, ref bool shapeIsCloned)
    {
        var occupied = GetOccupied();
        if (occupied.Count == 0)
        {
            log?.Debug("occupied is 0, skipping");
            base.OnTesselation(ref entityShape, shapePathForLogging, ref shapeIsCloned);
            return;
        }

        log?.Debug("=== OnTesselation START ===");

        if (!shapeIsCloned)
        {
            log?.Debug("Cloning shape (shapeIsCloned was false)");
            entityShape = entityShape.Clone();
            shapeIsCloned = true;
        }

        ICoreClientAPI? capi = this.Api as ICoreClientAPI;
        var voxShapeLoc = new AssetLocation("industrialrevolution:shapes/entity/steam-voxel.json");
        var root = this.Pos.AsBlockPos;

        IAsset? asset = capi?.Assets.TryGet(voxShapeLoc);
        Shape? voxShape = asset?.ToObject<Shape>();

        if (asset == null || voxShape == null || capi == null)
        {
            base.OnTesselation(ref entityShape, shapePathForLogging, ref shapeIsCloned);
            return;
        }

        foreach (var pos in occupied)
        {
            if (!pos.isFullBlock)
            {
                if (pos.steamGrid == null) log?.Warning("non fullblock has no steam grid");

                log?.Debug("chussy: " + util.SteamUtils.PosAsLocal(pos, this.Api));
                this.fillVoxels(
                    pos.steamGrid, pos, root,
                    voxShape, capi, voxShapeLoc,
                    ref entityShape, shapePathForLogging, ref shapeIsCloned
                );
            }
            else
            {
                voxShape = voxShape.Clone(); // make a copy

                // position the copy
                for (int i = 0; i < 3; i++)
                {
                    float offset = pos[i] - root[i];
                    voxShape.Elements[0].From[i] = offset * 16;
                    voxShape.Elements[0].To[i] = (offset + 1) * 16;
                }

                // bake it into the parent
                this.bake(
                    entityShape,
                    voxShape,
                    voxShapeLoc.ToString(),
                    shapePathForLogging,
                    capi
                );
            }
        }

        base.OnTesselation(ref entityShape, shapePathForLogging, ref shapeIsCloned);
        log?.Debug("=== OnTesselation END ===");
    }
}
