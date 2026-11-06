using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Goobstation.Common.Weapons.DelayedDash;

/// <summary>
/// Right click to dash towards a location, supports multiple hops. Just right click again during the do-after to add next hop destinations.
/// Spawns fire, if you want, or anything I guess, I'm not your mom.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DelayedDashComponent : Component
{
    [DataField]
    public TimeSpan WindupTime = TimeSpan.FromSeconds(1.5);

    [DataField]
    public SoundSpecifier? WindupSound;

    [DataField]
    public SoundSpecifier? DashSound;

    [DataField]
    public float DashSpeed = 40f;

    [DataField]
    public float MaxDashLength = 6f;

    /// <summary>
    /// How many hops you can do in total. Yes this includes the first one.
    /// </summary>
    [DataField]
    public int MaxDestinations = 2;

    [DataField]
    public EntProtoId? TrailPrototype;

    [DataField]
    public TimeSpan TrailInterval = TimeSpan.FromSeconds(0.025);
}

/// <summary>
/// Placed while winding up and dashing.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DelayedDashingComponent : Component
{
    [AutoNetworkedField]
    public EntityUid? Weapon;

    [AutoNetworkedField]
    public List<MapCoordinates> Destinations = new();

    [AutoNetworkedField]
    public bool Dashing;

    public bool Thrown;

    public TimeSpan LegEndTime;

    public TimeSpan NextLegTime;

    public HashSet<EntityUid> HitEntities = new();

    public List<string> ChangedFixtures = new();

    public TimeSpan NextTrailTime;
}

[Serializable, NetSerializable]
public sealed class DelayedDashRequestEvent(NetEntity weapon, NetCoordinates target) : EntityEventArgs
{
    public readonly NetEntity Weapon = weapon;
    public readonly NetCoordinates Target = target;
}

[Serializable, NetSerializable]
public sealed partial class DelayedDashDoAfterEvent : SimpleDoAfterEvent;
