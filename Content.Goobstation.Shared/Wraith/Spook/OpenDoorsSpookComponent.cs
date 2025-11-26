using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Spook;

[RegisterComponent, NetworkedComponent]
public sealed partial class OpenDoorsSpookComponent : Component
{
    [DataField]
    public float SearchRadius = 10f;

    [DataField]
    public int MaxContainer = 6;
}
