namespace Content.Shared._Lavaland.GPS;

/// <summary>
/// Component that is used for displaying GPS locator points on it's UI.
/// </summary>
[RegisterComponent]
public sealed partial class GpsSignalComponent : Component
{
    /// <summary>
    /// Name that is going to be displayed at GPS.
    /// </summary>
    [DataField]
    public LocId SignalName;
}
