using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Content.Shared.DoAfter;

namespace Content.Goobstation.Shared.Wraith.Revenant;


[RegisterComponent, NetworkedComponent]
public sealed partial class RevenantCrushComponent : Component
{
    /// <summary>
    ///  The duration of the doafter
    /// </summary>
    [DataField(required: true)]
    public TimeSpan AbilityDuration;

    /// <summary>
    ///  How much damage to deal to the entity before the doafter starts
    /// </summary>
    [DataField]
    public DamageSpecifier? InitialDamage = new();

    /// <summary>
    ///  How long to knockdown the user for
    /// </summary>
    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(2f);

    /// <summary>
    ///  Allowed distance for doafter between user and target
    /// </summary>
    [DataField]
    public float Distance = 15f;
}
