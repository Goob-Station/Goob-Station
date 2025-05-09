// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Managers;
using Content.Shared.Administration;
using Robust.Shared.Player;
using Robust.Shared.Toolshed;

namespace Content.Server.Administration.Toolshed;

[ToolshedCommand, AdminCommand(AdminFlags.Admin)]
public sealed class AdminsCommand : ToolshedCommand
{
    [Dependency] private readonly IAdminManager _admin = default!;

    [CommandImplementation("active")]
    public IEnumerable<ICommonSession> Active()
    {
        return _admin.ActiveAdmins;
    }

    [CommandImplementation("all")]
    public IEnumerable<ICommonSession> All()
    {
        return _admin.AllAdmins;
    }
}