using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Veil Ability
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingVeilComponent : Component
{
    public string? ActionGlare = "ActionVeil";

    [DataField]
    public float Range = 6f;
}
