using Content.Shared.FixedPoint;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared._TBDStation.ServerKarma.Events;

/// <summary>
///     Arguments for when a player's Karma is changed.
/// </summary>
[ByRefEvent]
public readonly record struct PlayerKarmaChangeEvent
{
    /// <summary>
    ///     Creates a new instance of this class.
    /// </summary>
    public PlayerKarmaChangeEvent(ICommonSession userSes, NetUserId userId, int newKarma, int oldKarma)
    {
        UserSes = userSes;
        UserID = userId;
        NewKarma = newKarma;
        OldKarma = oldKarma;
    }

    /// <summary>
    ///     ICommonSession of the player with the Karma change.
    /// </summary>
    public readonly ICommonSession UserSes;

    /// <summary>
    ///     NetUserId of the player with the Karma change.
    /// </summary>
    public readonly NetUserId UserID;

    /// <summary>
    ///     New amount that replaced the old one.
    /// </summary>
    public readonly int NewKarma;

    /// <summary>
    ///     Old amount that was replaced.
    /// </summary>
    public readonly int OldKarma;
}

[Serializable, NetSerializable]
public sealed class PlayerKarmaUpdateEvent : EntityEventArgs
{
    /// <summary>
    ///     Creates a new instance of this class.
    /// </summary>
    public PlayerKarmaUpdateEvent(int newKarma, int oldKarma)
    {
        NewKarma = newKarma;
        OldKarma = oldKarma;
    }

    /// <summary>
    ///     New amount that replaced the old one.
    /// </summary>
    public int NewKarma;

    /// <summary>
    ///     Old amount that was replaced.
    /// </summary>
    public int OldKarma;

}

/// <summary>
/// Department statistic delta(change) event
/// </summary>
[Serializable, NetSerializable]
public sealed class DepStatDEvent : EntityEventArgs
{
    public int Amount;
    public DepStatKey Type;
    public DepStatDEvent(int amount, DepStatKey type)
    {
        Amount = amount;
        Type = type;
    }
    /// <summary>
    /// Department Statistic key
    /// </summary>
    public enum DepStatKey
    {
        PowerOff,
        ResearchTech,
        LathePrint,
    }
}

[Serializable, NetSerializable]
public sealed class PlayerKarmaRequestEvent : EntityEventArgs
{
    /// <summary>
    ///     Creates a new instance of this class.
    /// </summary>
    public PlayerKarmaRequestEvent(){}
}

[Serializable, NetSerializable]
public sealed class PlayerKarmaHitEvent : EntityEventArgs
{
    /// <summary>
    ///     Creates a new instance of this class.
    /// </summary>
    public int Damage;
    public int User;
    public int Target;
    public PlayerKarmaHitEvent(FixedPoint2 damage, EntityUid user, EntityUid target)
    {
        Damage = (int) damage;
        User = user.Id;
        Target = target.Id;
    }
}


[Serializable, NetSerializable]
public sealed class PlayerKarmaGriefEvent : EntityEventArgs
{
    /// <summary>
    ///     Creates a new instance of this class.
    /// </summary>
    public int User;
    public GriefType Grief;
    // Used when a value can help determine the amount of karma lost.
    // Can be set to 0 and ignored in many cases.
    public float Val;
    public PlayerKarmaGriefEvent(EntityUid user, GriefType grief, float val = 0)
    {
        User = user.Id;
        Grief = grief;
        Val = val;
    }
    public enum GriefType
    {
        Explosion,
        IgniteOthers,
        OpenToxicCanister,
        Fire,
        Chemical, // Using chemical hazards
        Transform, // Transformin others into e.x. gondola
        Radiation, // Irradiating crewmembers
        Spawning, // Spawning dangerous mobs
    }
}
