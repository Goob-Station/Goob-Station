using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Sandevistan;

/// <summary>
/// This is the grey look the background gets during a sandevistan timestop.
/// Ignores player / item entities.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SandevistanTimestopVisionComponent : Component
{
    [DataField]
    public Color LightColor = new(0.13f, 0.13f, 0.15f, 1f);

    [DataField]
    public TimeSpan FadeIn = TimeSpan.FromSeconds(0.5f);

    [DataField]
    public TimeSpan StartedAt;
}
