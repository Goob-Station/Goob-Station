using Content.Server._MACRO.StrangeMoods.Eui;
using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._MACRO.StrangeMoods;

/// <summary>
///     Console commands associated with <see cref="StrangeMoodsSystem"/>.
/// </summary>
[AdminCommand(AdminFlags.Fun)]
public sealed partial class SharedMoodsCommand : LocalizedEntityCommands
{
    [Dependency] private IAdminManager _admin = default!;
    [Dependency] private EuiManager _eui = default!;
    [Dependency] private StrangeMoodsSystem _strangeMoods = default!;

    public override string Command => "sharedmoods";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        if (args.Length > 1)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 0), ("upper", 1)));
            return;
        }

        string? mood = null;

        if (args.Length == 1)
            mood = args[0];

        var ui = new SharedMoodsEui(_strangeMoods, _admin, _eui, player);
        _eui.OpenEui(ui, player);
        ui.SetMood(mood);
    }
}