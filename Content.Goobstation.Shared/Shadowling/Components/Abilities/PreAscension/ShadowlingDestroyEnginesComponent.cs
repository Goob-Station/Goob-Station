using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Destroy Engines ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingDestroyEnginesComponent : Component
{
    public string? ActionDestroyEngines = "ActionDestroyEngines";

    [DataField]
    public TimeSpan DelayTime = TimeSpan.FromMinutes(10);

    [DataField]
    public bool HasBeenUsed;
}
