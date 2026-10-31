using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Sandevistan;

/// <summary>
/// Applied to entities experiencing sandevistan time dilation.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SandevistanSlowdownVisionComponent : Component
{
    [DataField]
    public Color LightColor = new(0.14f, 0.58f, 0.22f, 0.42f);
}
