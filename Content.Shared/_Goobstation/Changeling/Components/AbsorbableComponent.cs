using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Changeling.Components;

/// <summary>
///     Component that indicates that a person can be absorbed by a changeling.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class AbsorbableComponent : Component
{
    /// <summary>
    ///     True if person's DNA has been absorbed by a changeling.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Absorbed = false;

    /// <summary>
    ///     Group of damage that will be dealed to absorbed person / ling on death
    /// </summary>
    public ProtoId<DamageGroupPrototype> AbsorbedDamageGroup = "Genetic";
}
