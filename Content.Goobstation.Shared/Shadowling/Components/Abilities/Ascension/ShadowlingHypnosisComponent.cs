using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for the Hypnosis ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingHypnosisComponent : Component
{
    public string? HypnosisAction = "ActionHynosis";
}
