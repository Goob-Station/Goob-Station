// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.Mind;

namespace Content.Shared.Administration;

[Serializable, NetSerializable]
public sealed record PlayerInfo(
    string Username,
    string CharacterName,
    string IdentityName,
    string StartingJob,
    bool Antag,
    ProtoId<RoleTypePrototype>? RoleProto,
    LocId? Subtype,
    int SortWeight,
    NetEntity? NetEntity,
    NetUserId SessionId,
    bool Connected,
    bool ActiveThisRound,
    bool IsGhost, // Goobstation
    TimeSpan? OverallPlaytime)
{
    private string? _playtimeString;

    public bool IsPinned { get; set; }

    public string PlaytimeString => _playtimeString ??=
        OverallPlaytime?.ToString("%d':'hh':'mm") ?? Loc.GetString("generic-unknown-title");

    public bool Equals(PlayerInfo? other)
    {
        return other?.SessionId == SessionId;
    }

    public override int GetHashCode()
    {
        return SessionId.GetHashCode();
    }
}
