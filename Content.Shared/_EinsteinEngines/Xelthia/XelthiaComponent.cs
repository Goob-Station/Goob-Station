using Robust.Shared.GameStates;


namespace Content.Shared._EinsteinEngines.Xelthia;


/// <summary>
/// This is eventually going to be used for the arm regrowth action the Xelthia have. I have no idea how to code this.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class XelthiaComponent : Component
{
    [DataField]
    public TimeSpan UseDelay { get; set; } = TimeSpan.FromMinutes(5);

    [DataField]
    public EntityUid? XelthiaRegenerateAction;
}
