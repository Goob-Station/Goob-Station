// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Motd;

/// <summary>
/// A command that can be used by any player to print the Message of the Day.
/// </summary>
[AnyCommand]
public sealed class GetMotdCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override string Command => "get-motd";
    
    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _entityManager.EntitySysManager.GetEntitySystem<MOTDSystem>().TrySendMOTD(shell);
    }
}