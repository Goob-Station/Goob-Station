// SPDX-FileCopyrightText: 2023 Errant <35878406+dmnct@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kot <1192090+koteq@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Administration.Managers;
using Content.Shared.CCVar;
using Robust.Client;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;

namespace Content.Client.DebugMon;

/// <summary>
/// This handles preventing certain debug monitors from being usable by non-admins.
/// </summary>
internal sealed class DebugMonitorManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IClientAdminManager _admin = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterface = default!;
    [Dependency] private readonly IBaseClient _baseClient = default!;

    public void FrameUpdate()
    {
        if (_baseClient.RunLevel == ClientRunLevel.InGame
            && !_admin.IsActive()
            && _cfg.GetCVar(CCVars.DebugCoordinatesAdminOnly))
        {
            _userInterface.DebugMonitors.SetMonitor(DebugMonitor.Coords, false);
        }
    }
}