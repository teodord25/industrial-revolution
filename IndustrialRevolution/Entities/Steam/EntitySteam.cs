using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

using System.Collections.Generic;
using Vintagestory.API.MathTools;
using IndustrialRevolution.util;

namespace IndustrialRevolution.Entities.Steam;

internal partial class EntitySteam : EntityAgent
{
    private HashSet<SteamPos> occupied = new HashSet<SteamPos>();
    private Queue<(int x, int y, int z)> toCheck =
        new Queue<(int x, int y, int z)>();

    private SteamVolume volume = SteamVolume.FromVoxels(0);
    private SteamVolume? maxVol = SteamVolume.FromBlocks(10);

    private ModLogger? log = IndustrialRevolutionModSystem.Logger;
    private HashSet<BlockPos> occupiedVoxels = new HashSet<BlockPos>();

    public override void Initialize(
        EntityProperties properties, ICoreAPI api, long InChunkIndex3d
    )
    {
        base.Initialize(properties, api, InChunkIndex3d);

        if (api.Side == EnumAppSide.Server) return;

        WatchedAttributes.RegisterModifiedListener("steamShapeVersion", () =>
        {
            log?.Debug(
                "Client detected steamShapeVersion change," +
                " marking shape modified"
            );
            this.MarkShapeModified();
        });
    }

    public override void OnInteract(
        EntityAgent byEntity, ItemSlot itemslot,
        Vec3d hitPosition, EnumInteractMode mode
    )
    {
        if (mode != EnumInteractMode.Interact) return;

        ExpandSteam();

        this.MarkShapeModified();
    }
}
