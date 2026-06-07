using Content.Shared._White.RadialSelector;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components.Mobs;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChooseVoidCreatureComponent : Component
{
    /// <summary>
    /// List of summonable void creatures to show in the radial menu.
    /// </summary>
    [DataField(required: true)]
    public List<RadialSelectorEntry> AvailableSummons = new();
}
