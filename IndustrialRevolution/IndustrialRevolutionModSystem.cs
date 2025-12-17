using Vintagestory.API.Common;
using IndustrialRevolution.Blocks;

namespace IndustrialRevolution;

public class IndustrialRevolutionModSystem : ModSystem
{
    public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass(Mod.Info.ModID + "." + "boiler", typeof(BlockBoiler));

            api.World.Logger.Event("started 'Industrial Revolution' mod");
        }
}
