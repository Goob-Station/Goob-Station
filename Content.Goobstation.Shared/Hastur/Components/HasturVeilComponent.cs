using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Hastur.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class HasturVeilComponent : Component
{
    /// <summary>
    /// If true, veil is currently active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsActive;
}
