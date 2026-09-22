using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Gives a vhs overlay to any entity that has it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherIncorporealOverlayComponent : Component
{
    [DataField, AutoNetworkedField]
    public float FadeSpeed = 2.2f;
}
