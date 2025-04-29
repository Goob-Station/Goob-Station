// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.Administration;
using Content.Server.Polymorph.Systems;
using Content.Shared.Administration;
using Robust.Shared.Toolshed;

namespace Content.Server.Polymorph.Toolshed;

/// <summary>
///     Undoes a polymorph, reverting the target to it's original form.
/// </summary>
[ToolshedCommand, AdminCommand(AdminFlags.Fun)]
public sealed class UnpolymorphCommand : ToolshedCommand
{
    private PolymorphSystem? _system;

    [CommandImplementation]
    public EntityUid? Unpolymorph([PipedArgument] EntityUid input)
    {
        _system ??= GetSys<PolymorphSystem>();

        return _system.Revert(input);
    }

    [CommandImplementation]
    public IEnumerable<EntityUid> Unpolymorph([PipedArgument] IEnumerable<EntityUid> input)
        => input.Select(Unpolymorph).Where(x => x is not null).Select(x => (EntityUid)x!);
}