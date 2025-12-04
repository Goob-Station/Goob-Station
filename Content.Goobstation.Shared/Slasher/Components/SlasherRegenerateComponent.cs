using Content.Shared.Actions;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Basically just injects whatever chemical you want and breaks cuffs.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherRegenerateComponent : Component
{
    [ViewVariables]
    public EntityUid? ActionEnt;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherRegenerate";

    /// <summary>
    /// The reagent to inject
    /// </summary>
    [DataField("reagent")]
    public ProtoId<ReagentPrototype> Reagent = "slasherium";

    /// <summary>
    /// How much reagent to inject
    /// </summary>
    [DataField("reagentAmount")]
    public float ReagentAmount = 10f;
}


