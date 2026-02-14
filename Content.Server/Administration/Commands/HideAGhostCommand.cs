using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Stealth;
using Content.Shared.Administration;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Stealth.Components;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Serilog;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class HideAGhostCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entities = default!;

    public override string Command => "hideaghost";
    public override string Help => "hideaghost";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 0)
        {
            shell.WriteError(LocalizationManager.GetString("shell-need-exactly-zero-arguments"));
            return;
        }
        var player = shell.Player;
        if (player?.AttachedEntity == null)
        {
            shell.WriteError(LocalizationManager.GetString("shell-must-be-attached-to-entity"));
            return;
        }
        var stealthSystem = _entities.System<StealthSystem>();
        var ghostEnt = player.AttachedEntity.Value;
        if (!_entities.HasComponent<GhostComponent>(ghostEnt))
        {
           return;
        }
        if (_entities.HasComponent<StealthComponent>(ghostEnt))
        {
            _entities.RemoveComponentDeferred<StealthComponent>(ghostEnt);
            return;
        }
        _entities.AddComponent<StealthComponent>(ghostEnt);
        stealthSystem.SetVisibility(ghostEnt, -1.5f);
        stealthSystem.SetEnabled(ghostEnt, true);
    }
}
