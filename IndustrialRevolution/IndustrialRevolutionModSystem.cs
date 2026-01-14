using Vintagestory.API.Common;
using System.Collections.Generic;
using Vintagestory.API.MathTools;

using IndustrialRevolution.util;
using IndustrialRevolution.Blocks;
using IndustrialRevolution.BlockEntities;
using IndustrialRevolution.Entities.Steam;

namespace IndustrialRevolution;

public class IndustrialRevolutionModSystem : ModSystem
{
    public static ModLogger? Logger;

    public override void StartPre(ICoreAPI api)
    {
        Logger = new ModLogger(api, "industrial-revolution-main");
        Logger.Info($"Mod started on side: {api.Side}");
        api.Logger.Event($"IR Logger created on {api.Side}");
    }

    private void RegisterComplexBlocks(ICoreAPI api) {
        api.RegisterBlockClass(Mod.Info.ModID + "." + "blockboiler", typeof(BlockBoiler));
    }

    private void RegisterComplexEntities(ICoreAPI api) {
        api.RegisterEntity(Mod.Info.ModID + "." + "entitysteam", typeof(EntitySteam));
    }

    private void RegiterBlockEntities(ICoreAPI api) {
        api.RegisterBlockEntityClass(Mod.Info.ModID + "." + "entityboiler", typeof(BlockEntityBoiler));
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        this.RegisterComplexEntities(api);
        this.RegisterComplexBlocks(api);
        this.RegiterBlockEntities(api);

        api.World.Logger.Event("started 'Industrial Revolution' mod");
    }
}
