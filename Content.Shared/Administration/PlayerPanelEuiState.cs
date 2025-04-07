// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Eui;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration;

[Serializable, NetSerializable]
public sealed class PlayerPanelEuiState(NetUserId guid,
    string username,
    TimeSpan playtime,
    int? totalNotes,
    int? totalBans,
    int? totalRoleBans,
    int sharedConnections,
    bool? whitelisted,
    bool canFreeze,
    bool frozen,
    bool canAhelp)
    : EuiStateBase
{
    public readonly NetUserId Guid = guid;
    public readonly string Username = username;
    public readonly TimeSpan Playtime = playtime;
    public readonly int? TotalNotes = totalNotes;
    public readonly int? TotalBans = totalBans;
    public readonly int? TotalRoleBans = totalRoleBans;
    public readonly int SharedConnections = sharedConnections;
    public readonly bool? Whitelisted = whitelisted;
    public readonly bool CanFreeze = canFreeze;
    public readonly bool Frozen = frozen;
    public readonly bool CanAhelp = canAhelp;
}


[Serializable, NetSerializable]
public sealed class PlayerPanelFreezeMessage : EuiMessageBase
{
    public readonly bool Mute;

    public PlayerPanelFreezeMessage(bool mute = false)
    {
        Mute = mute;
    }
}

[Serializable, NetSerializable]
public sealed class PlayerPanelLogsMessage : EuiMessageBase;

[Serializable, NetSerializable]
public sealed class PlayerPanelDeleteMessage : EuiMessageBase;

[Serializable, NetSerializable]
public sealed class PlayerPanelRejuvenationMessage: EuiMessageBase;