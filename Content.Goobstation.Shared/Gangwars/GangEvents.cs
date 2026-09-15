using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Gangwars.Events;

[Serializable, NetSerializable]
public enum GangSprayCanVisuals : byte
{
    Empty,
}

[Serializable, NetSerializable]
public sealed partial class GangSprayCanDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public NetCoordinates ClickLocation;

    [DataField]
    public Color Gang;
}

/// <summary>
/// Literally just stops the gang signs from appearing white for a second on spawn.
/// </summary>
[ByRefEvent]
public readonly record struct GangColorAppliedEvent;

/// <summary>
/// Raised when a gang member uses their toggle territory overlay action.
/// </summary>
public sealed partial class GangMemberToggleOverlayEvent : InstantActionEvent;

/// <summary>
/// Raised when the Gang Leader uses their locker summon / relocate action.
/// </summary>
public sealed partial class GangLeaderSummonLockerEvent : InstantActionEvent;

/// <summary>
/// Raised when the Gang Leader uses their member offer action on a target entity.
/// </summary>
public sealed partial class GangLeaderMemberOfferEvent : EntityTargetActionEvent;

/// <summary>
/// Sent by the server to the owning client when the gang leader needs to pick a color before their locker can be summoned.
/// </summary>
[Serializable, NetSerializable]
public sealed class GangLeaderNeedsColorPickEvent(Dictionary<Color, string> gangNames) : EntityEventArgs
{
    public Dictionary<Color, string> GangNames { get; } = gangNames;
}

[Serializable, NetSerializable]
public sealed class GangColorChosenEvent(Color color, string gangName) : EntityEventArgs
{
    public Color ChosenColor { get; } = color;
    public string GangName { get; } = gangName;
}

/// <summary>
/// Sent by the server to the target client when a gang leader offers them membership.
/// </summary>
[Serializable, NetSerializable]
public sealed class GangInviteOfferEvent(string leaderName, Color gangColor, NetEntity leaderEntity, string gangName) : EntityEventArgs
{
    public string LeaderName { get; } = leaderName;
    public Color GangColor { get; } = gangColor;
    public NetEntity LeaderEntity { get; } = leaderEntity;
    public string GangName { get; } = gangName;
}

/// <summary>
/// Sent by the invited client back to the server with their accept/deny choice.
/// </summary>
[Serializable, NetSerializable]
public sealed class GangInviteResponseEvent(bool accepted, NetEntity leaderEntity) : EntityEventArgs
{
    public bool Accepted { get; } = accepted;
    public NetEntity LeaderEntity { get; } = leaderEntity;
}

public sealed class GangMemberRecruitedEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed partial class GangDuffelBagUntrapDoAfterEvent : SimpleDoAfterEvent;

[DataDefinition]
public sealed partial class GangTipOffEvent : EntityEventArgs;
