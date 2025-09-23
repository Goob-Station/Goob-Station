using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Content.Shared.DoAfter;

namespace Content.Goobstation.Shared.Wraith.Revenant;


[RegisterComponent, NetworkedComponent]
public sealed partial class RevenantCrushComponent : Component
{
    [DataField(required: true)]
    public TimeSpan AbilityDuration;

    [DataField]
    public DamageSpecifier InitialDamage;

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(2f);

    [DataField]
    public float MovementThreshold = 0.3f;
}
