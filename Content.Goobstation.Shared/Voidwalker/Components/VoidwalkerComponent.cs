using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Voidwalker.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class VoidwalkerComponent : Component
{
    /// <summary>
    /// If the voidwalker is within this much of a passed object, don't count it as being in space.
    /// This is to prevent being able to stand inside a passed object, since they have no atmosphere inside.
    /// If you can think of a better way to handle this, do tell me - delph
    /// </summary>
    public float PassedObjectGraceRange = 1.0f; //

    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan NextHealingTick;

    [DataField]
    public TimeSpan HealingTickInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How much to heal the voidwalker by when they're spaced.
    /// </summary>
    [DataField]
    public DamageSpecifier HealingWhenSpaced = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Brute", -22.5},
            { "Burn", -22.5},
            { "Airloss", -10.5},
            { "Heat", -7.5},
            { "Shock", -7.5},
        },
    };

    [DataField]
    public string TrackedComponentsIdentifier = "Voidwalker";

    /// <summary>
    /// What to multiply the voidwalker's speed by when they're in a non-spaced area.
    /// </summary>
    [DataField]
    public float NonSpacedSpeedModifier = 0.6f;

    [DataField]
    public EntProtoId CosmicSkull = "CosmicSkull";

}
