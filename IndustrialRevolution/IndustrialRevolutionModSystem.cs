using Vintagestory.API.Common;

namespace IndustrialRevolution;

public class IndustrialRevolutionModSystem : ModSystem
{
    public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("BlockBoilerBasic", typeof(BlockBoilerBasic));
        }
}
