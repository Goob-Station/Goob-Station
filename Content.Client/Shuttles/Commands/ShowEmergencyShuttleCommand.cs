// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Shuttles.Systems;
using Robust.Shared.Console;

namespace Content.Client.Shuttles.Commands;

public sealed class ShowEmergencyShuttleCommand : IConsoleCommand
{
    public string Command => "showemergencyshuttle";
    public string Description => "Shows the expected position of the emergency shuttle";
    public string Help => $"{Command}";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var tstalker = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ShuttleSystem>();
        tstalker.EnableShuttlePosition ^= true;
        shell.WriteLine($"Set emergency shuttle debug to {tstalker.EnableShuttlePosition}");
    }
}