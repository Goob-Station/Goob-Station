// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.NPC.HTN;
using Robust.Shared.Console;

namespace Content.Client.NPC;

public sealed class ShowHTNCommand : IConsoleCommand
{
    public string Command => "showhtn";
    public string Description => "Shows the current status for HTN NPCs";
    public string Help => $"{Command}";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var npcs = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<HTNSystem>();
        npcs.EnableOverlay ^= true;
    }
}