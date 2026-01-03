using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using System.Linq;
using Vintagestory.API.Util;

using System.Collections.Generic;
using Vintagestory.API.MathTools;
using IndustrialRevolution.util;

namespace IndustrialRevolution.Entities;

internal partial class EntitySteam : EntityAgent
{

    private ModLogger? log = IndustrialRevolutionModSystem.Logger;
    private HashSet<BlockPos> occupiedVoxels = new HashSet<BlockPos>();

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
}
