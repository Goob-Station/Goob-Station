// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Administration;
using Robust.Shared.Toolshed;

namespace Content.Server.Administration.Toolshed;

[ToolshedCommand, AnyCommand]
public sealed class MarkedCommand : ToolshedCommand
{
    [CommandImplementation]
    public IEnumerable<EntityUid> Marked(IInvocationContext ctx)
    {
        var marked = ctx.ReadVar("marked") as IEnumerable<EntityUid>;
        return marked ?? Array.Empty<EntityUid>();
    }
}