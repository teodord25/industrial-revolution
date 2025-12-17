using Vintagestory.API.Common;
using IndustrialRevolution.Blocks;

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

        api.RegisterBlockClass(Mod.Info.ModID + "." + "boiler", typeof(BlockBoiler));

        api.World.Logger.Event("started 'Industrial Revolution' mod");
    }
}
