using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Surgery.Steps;

// TODO: move this to shitmed module if someone ever does it
/// <summary>
/// Deals damage to the surgeon when a step is done.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SurgeryDamageUserSystem))]
public sealed partial class SurgeryDamageUserComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Popup shown to everyone, gets passed "target" and "part"
    /// </summary>
    [DataField]
    public LocId? Popup;
}
