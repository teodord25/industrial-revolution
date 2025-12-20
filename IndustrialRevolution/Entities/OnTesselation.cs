using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace IndustrialRevolution.Entities;

internal partial class EntitySteam : EntityAgent
{
    public HashSet<BlockPos> GetOccupiedVoxels()
    {
        if (!WatchedAttributes.GetBool("steam-touched")) return [];
        WatchedAttributes.MarkPathDirty("steam-touched");

        byte[] data = WatchedAttributes.GetBytes("steam-occupied");
        if (data == null) return new HashSet<BlockPos>();

        int[] coords = SerializerUtil.Deserialize<int[]>(data);
        var positions = new HashSet<BlockPos>();

        for (int i = 0; i < coords.Length; i += 3)
        {
            positions.Add(new BlockPos(coords[i], coords[i + 1], coords[i + 2]));
        }

        return positions;
    }

    protected override void OnTesselation(ref Shape entityShape, string shapePathForLogging, ref bool shapeIsCloned)
    {
        var occupied = GetOccupiedVoxels();
        if (occupied.Count == 0)
        {
            base.OnTesselation(ref entityShape, shapePathForLogging, ref shapeIsCloned);
            return;
        }
        ;

        log?.Debug("=== OnTesselation START ===");

        if (!shapeIsCloned)
        {
            log?.Debug("Cloning shape (shapeIsCloned was false)");
            entityShape = entityShape.Clone();
            shapeIsCloned = true;
        }

        ICoreClientAPI? capi = this.Api as ICoreClientAPI;

        var shapeLoc = new AssetLocation("industrialrevolution:shapes/entity/steam-voxel.json");
        var pos = this.Pos.AsBlockPos;

        foreach (var voxel in occupied)
        {
            IAsset? asset = capi.Assets.TryGet(shapeLoc);

            Shape? partShape = asset.ToObject<Shape>();
            if (
                    asset == null ||
                    partShape == null ||
                    capi == null
            )
            {
                base.OnTesselation(ref entityShape, shapePathForLogging, ref shapeIsCloned);
                return;
            }

            for (int i = 0; i < 3; i++) {
                partShape.Elements[0].From[i] += voxel[i] - pos[i];
                partShape.Elements[0].To[i] += (voxel[i] + 1) - (pos[i] + 1);
            }

            entityShape.StepParentShape(
                partShape,
                shapeLoc.ToString(),
                shapePathForLogging,
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

            base.OnTesselation(ref entityShape, shapePathForLogging, ref shapeIsCloned);
            log?.Debug("=== OnTesselation END ===");
        }
    }
}
