using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._White.ListViewSelector;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class DefileComponent : Component
{
    /// <summary>
    /// Dictionary of reagents and their quantities to be injected.
    /// Key: Reagent ID, Value: Quantity to inject.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<EntProtoId, FixedPoint2> Reagents = new();

    [ViewVariables, AutoNetworkedField]
    public List<ListViewSelectorEntry> ReagentsEntryList = new();

    [DataField, AutoNetworkedField]
    public EntProtoId? ReagentSelected;

    [DataField, AutoNetworkedField]
    public FixedPoint2 ReagentSelectedAmount;
}
