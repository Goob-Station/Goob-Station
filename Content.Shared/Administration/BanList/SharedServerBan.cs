// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration.BanList;

[Serializable, NetSerializable]
public record SharedServerBan(
    int? Id,
    NetUserId? UserId,
    (string address, int cidrMask)? Address,
    string? HWId,
    DateTime BanTime,
    DateTime? ExpirationTime,
    string Reason,
    string? BanningAdminName,
    SharedServerUnban? Unban
);