using Content.Shared._White.RadialSelector;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SummonVoidCreatureComponent : Component
{
    /// <summary>
    /// List of summonable void creatures to show in the radial menu.
    /// </summary>
    [DataField(required: true)]
    public List<RadialSelectorEntry> AvailableSummons = new();

    /// <summary>
    /// The action entity spawned for this component.
    /// </summary>
    [ViewVariables]
    public EntityUid? ActionEnt;

    /// <summary>
    /// Prototype ID for the radial summon action.
    /// </summary>
    [ViewVariables]
    public EntProtoId ActionId = "ActionSummonVoidCreature";
}
