// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Administration;
using Content.Server.Polymorph.Systems;
using Content.Shared.Administration;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;

namespace Content.Server.Polymorph.Toolshed;

/// <summary>
///     Polymorphs the given entity(s) into the target morph.
/// </summary>
[ToolshedCommand, AdminCommand(AdminFlags.Fun)]
public sealed class PolymorphCommand : ToolshedCommand
{
    private PolymorphSystem? _system;
    [Dependency] private IPrototypeManager _proto = default!;

    [CommandImplementation]
    public EntityUid? Polymorph(
            [PipedArgument] EntityUid input,
            ProtoId<PolymorphPrototype> protoId
        )
    {
        _system ??= GetSys<PolymorphSystem>();

        if (!_proto.TryIndex(protoId, out var prototype))
            return null;

        return _system.PolymorphEntity(input, prototype.Configuration);
    }

    [CommandImplementation]
    public IEnumerable<EntityUid> Polymorph(
            [PipedArgument] IEnumerable<EntityUid> input,
            ProtoId<PolymorphPrototype> protoId
        )
        => input.Select(x => Polymorph(x, protoId)).Where(x => x is not null).Select(x => (EntityUid)x!);
}