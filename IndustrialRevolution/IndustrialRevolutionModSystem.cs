using Vintagestory.API.Common;
using System.Collections.Generic;
using Vintagestory.API.MathTools;

using IndustrialRevolution.util;
using IndustrialRevolution.Blocks;
using IndustrialRevolution.BlockEntities;
using IndustrialRevolution.Entities;

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

    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        api.RegisterBlockClass(Mod.Info.ModID + "." + "blockboiler", typeof(BlockBoiler));
        api.RegisterEntity(Mod.Info.ModID + "." + "entitysteam", typeof(EntitySteam));
        api.RegisterBlockEntityClass(Mod.Info.ModID + "." + "entityboiler", typeof(BlockEntityBoiler));

        api.Network
            .RegisterChannel("steam-occupied")
            .RegisterMessageType<HashSet<BlockPos>>();

        api.World.Logger.Event("started 'Industrial Revolution' mod");
    }
}
