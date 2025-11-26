using Content.Shared._White.RadialSelector;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpookComponent : Component
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Actions = new();
}
