using Content.Shared.Damage;
using Content.Shared._Shitmed.Targeting; // Shitmed

namespace Content.Server.Damage.Components;

[RegisterComponent]
public sealed partial class DamageUserOnTriggerComponent : Component
{
    [DataField("ignoreResistances")] public bool IgnoreResistances;

    [DataField("damage", required: true)]
    public DamageSpecifier Damage = default!;

    /// <summary>
    /// Shitmed Change: Lets mousetraps, etc. target the feet.
    /// </summary>
    [DataField]
    public TargetBodyPart? TargetPart = TargetBodyPart.Feet;
}
