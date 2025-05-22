using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Pain.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class NerveComponent : Component
{
    // Yuh-uh
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 PainMultiplier = 1.0f;

    // How feel able the pain is; The value can be decreased by pain suppressants and Nerve Damage.
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 PainFeels => 1f + PainFeelingModifiers.Values.Sum(modifier => (float) modifier.Change);

    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<(EntityUid, string), PainFeelingModifier> PainFeelingModifiers = new();

    /// <summary>
    /// Nerve system, to which this nerve is parented.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid ParentedNerveSystem;
}
