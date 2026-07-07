using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.Wetness.Components;

/// <summary>
/// Clothing that can absorb clean water.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WettableComponent : Component
{
    /// <summary>Current absorbed water, in units.</summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 Wetness;

    /// <summary>Maximum absorbed water.</summary>
    [DataField]
    public FixedPoint2 MaxWetness = FixedPoint2.New(20);

    /// <summary>Wetness required for the droplet visual.</summary>
    [DataField]
    public FixedPoint2 VisualThreshold = FixedPoint2.New(5);

    // Drying and dripping.
    [DataField]
    public FixedPoint2 DryPerStep = FixedPoint2.New(1);

    [DataField]
    public float DryIntervalMin = 10f;

    [DataField]
    public float DryIntervalMax = 18f;

    [DataField]
    public float DripChance = 0.15f;

    [DataField]
    public FixedPoint2 DripAmount = FixedPoint2.New(2);

    /// <summary>Next drying step time.</summary>
    [DataField, AutoNetworkedField]
    public TimeSpan NextDryTime;

    // Wringing.
    [DataField]
    public float WringDoAfterDuration = 3f;

    [DataField]
    public SoundSpecifier WringSound = new SoundPathSpecifier("/Audio/_Pirate/Machines/wring.ogg");
}

[Serializable, NetSerializable]
public sealed partial class WringWetnessDoAfterEvent : SimpleDoAfterEvent;
