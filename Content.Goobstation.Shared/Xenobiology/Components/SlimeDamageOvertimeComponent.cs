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
    public EntityUid SourceEntityUid { get; set; }

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Blunt", 2.5},
        }
    };

    [DataField("interval", customTypeSerializer: typeof(TimespanSerializer))]
    public TimeSpan Interval = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan NextTickTime = TimeSpan.Zero;
}
