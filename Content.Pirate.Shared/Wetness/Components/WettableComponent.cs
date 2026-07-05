using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.Wetness.Components;

/// <summary>
/// Clothing that can absorb clean water. Wetness is a scalar field, tracked separately from the
/// stain solution (see <see cref="Content.Pirate.Shared.Stains.Components.StainableComponent"/>).
/// The first pass only ever stores water, so a plain field is used instead of a solution.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WettableComponent : Component
{
    /// <summary>Current absorbed water, in units. Default dry.</summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 Wetness;

    /// <summary>Design default capacity; override per item only when needed.</summary>
    [DataField]
    public FixedPoint2 MaxWetness = FixedPoint2.New(15);

    /// <summary>At or above this wetness the worn droplet visual shows.</summary>
    [DataField]
    public FixedPoint2 VisualThreshold = FixedPoint2.New(5);

    // Drying / dripping tunables (design baselines).
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

    /// <summary>Server-driven schedule for the next drying step.</summary>
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
