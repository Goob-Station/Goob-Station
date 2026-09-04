using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Just for pop-ups lol
/// i guess the infested thing can also go here.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TerrorWebComponent : Component
{
    [DataField]
    public bool InflictsInfested;
}
