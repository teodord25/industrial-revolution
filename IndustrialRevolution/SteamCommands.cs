using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using IndustrialRevolution.Entities;
using IndustrialRevolution.util;

namespace IndustrialRevolution;

public class SteamCommandMod : ModSystem
{
    private ICoreServerAPI? api;
    public ModLogger? log = IndustrialRevolutionModSystem.Logger;

    public override bool ShouldLoad(EnumAppSide side)
    {
        return side == EnumAppSide.Server;
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        this.api = api;

        var steamCmd = api.ChatCommands.Create("steam")
            .WithDescription("Manage steam entities")
            .RequiresPrivilege(Privilege.chat)
            .RequiresPlayer();

        steamCmd.BeginSubCommand("status")
            .WithDescription("Display status of nearest steam entity")
            .HandleWith(OnSteamStatus)
            .EndSubCommand();

        steamCmd.BeginSubCommand("next")
            .WithDescription("Trigger next expansion step on nearest steam entity")
            .WithArgs(api.ChatCommands.Parsers.OptionalInt("steps", 1))
            .HandleWith(OnSteamNext)
            .EndSubCommand();
    }

    private TextCommandResult OnSteamStatus(TextCommandCallingArgs args)
    {
        var player = args.Caller.Entity;
        var world = player.World;

        Entity steamEntity = world.GetNearestEntity(
            player.Pos.XYZ,
            10f,
            5f,
            (entity) => entity.Code?.Path == "steam"
        );

        if (steamEntity == null)
        {
            return TextCommandResult.Success("No steam entity found nearby.");
        }

        int expansionLevel = steamEntity.WatchedAttributes.GetInt("expansionLevel", 0);
        float steamPressure = steamEntity.WatchedAttributes.GetFloat("steamPressure", 0f);
        long lastExpansionTime = steamEntity.WatchedAttributes.GetLong("lastExpansionTime", 0);

        string message = string.Format(
            "Steam Entity Status:\n" +
            "- Entity ID: {0}\n" +
            "- Position: {1}\n" +
            "- Expansion Level: {2}\n" +
            "- Steam Pressure: {3:F2}\n" +
            "- Last Expansion: {4} ms ago",
            steamEntity.EntityId,
            steamEntity.Pos.AsBlockPos,
            expansionLevel,
            steamPressure,
            world.ElapsedMilliseconds - lastExpansionTime
        );

        return TextCommandResult.Success(message);
    }


    private TextCommandResult OnSteamNext(TextCommandCallingArgs args)
    {
        var player = args.Caller.Entity;
        var world = player.World;

        int steps = (int)args[0];

        Entity steamEntity = world.GetNearestEntity(
            player.Pos.XYZ,
            10f,
            5f,
            (entity) => entity.Code?.Path == "steam"
        );

        if (steamEntity == null)
        {
            return TextCommandResult.Success("No steam entity found nearby.");
        }

        if (steamEntity is EntitySteam steam)
        {
            steam.ExpandSteam();
            return TextCommandResult.Success($"Triggered {steps} expansion step(s) on steam entity.");
        }

        return TextCommandResult.Success("Entity is not a steam entity.");
    }
}
