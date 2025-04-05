using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wizard.Simians;

[RegisterComponent, NetworkedComponent]
public sealed partial class DropItemsOnTimedDespawnComponent : Component
{
    [DataField]
    public bool DropDespawningItems;
}
