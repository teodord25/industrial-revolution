using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace IndustrialRevolution.Entities;

internal partial class EntitySteam : EntityAgent
{
    // TODO: only draw edge voxels, somehow, maybe through toCheck or something
    // because drawing in occluded voxels for chiseled blocks could get crazy...
    public HashSet<BlockPos> GetOccupied()
    {
        byte[] data = WatchedAttributes.GetBytes("steamOccupied");
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
        var shapeLoc = new AssetLocation("industrialrevolution:shapes/entity/steam-voxel.json");
        var pos = this.Pos.AsBlockPos;

        foreach (var voxel in occupied)
        {
            IAsset? asset = capi?.Assets.TryGet(shapeLoc);
            Shape? partShape = asset?.ToObject<Shape>();

            if (asset == null || partShape == null || capi == null)
            {
                base.OnTesselation(ref entityShape, shapePathForLogging, ref shapeIsCloned);
                return;
            }

            partShape = partShape.Clone();

            for (int i = 0; i < 3; i++)
            {
                // TODO: use this From and To stuff to modify existing
                // segments rather than adding a voxel shape for every voxel
                float offset = voxel[i] - pos[i];
                partShape.Elements[0].From[i] = offset * 16;
                partShape.Elements[0].To[i] = (offset + 1) * 16;
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
        }

        base.OnTesselation(ref entityShape, shapePathForLogging, ref shapeIsCloned);
        log?.Debug("=== OnTesselation END ===");
    }
}
