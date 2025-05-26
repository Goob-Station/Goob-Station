using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Goobstation.Shared.Xenobiology.Components;

/// <summary>
/// This is used for slime latching damage, this can be expanded in the future to allow for special breed dependent effects.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlimeDamageOvertimeComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? SourceEntityUid;

    [DataField(customTypeSerializer: typeof(TimespanSerializer))]
    public TimeSpan Interval = TimeSpan.FromSeconds(1);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextTickTime = TimeSpan.Zero;

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 2.5},
        },
    };
}
