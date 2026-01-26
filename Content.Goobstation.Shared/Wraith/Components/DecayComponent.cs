using Content.Goobstation.Common.Emag.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DecayComponent : Component
{
    /// <summary>
    /// How much stamina damage to apply over time.
    /// </summary>
    [DataField]
    public float StaminaDamageAmount = 150f;

    /// <summary>
    /// What emag interaction to use
    /// </summary>
    [DataField]
    public List<ProtoId<EmagTypePrototype>> EmagType = ["Access", "Interaction"];
}
