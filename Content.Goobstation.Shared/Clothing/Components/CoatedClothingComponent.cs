using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Clothing.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CoatedClothingComponent : Component
{
    public List<string> CoatingNames = new List<string>();
}
