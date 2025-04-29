// SPDX-FileCopyrightText: 2022 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Fluids;

[AdminCommand(AdminFlags.Debug)]
public sealed class ShowFluidsCommand : IConsoleCommand
{
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    public string Command => "showfluids";
    public string Description => "Toggles seeing puddle debug overlay.";
    public string Help => $"Usage: {Command}";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;
        if (player == null)
        {
            shell.WriteLine("You must be a player to use this command.");
            return;
        }

        var fluidDebug = _entitySystem.GetEntitySystem<PuddleDebugDebugOverlaySystem>();
        var enabled = fluidDebug.ToggleObserver(player);

        shell.WriteLine(enabled
            ? "Enabled the puddle debug overlay."
            : "Disabled the puddle debug overlay.");
    }
}