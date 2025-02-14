using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.Simians;

[RegisterComponent, NetworkedComponent]
public sealed partial class DropItemsOnTimedDespawnComponent : Component
{
    [DataField]
    public bool DropDespawningItems;
}
