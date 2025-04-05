using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Wizard;

[RegisterComponent, NetworkedComponent]
public sealed partial class ApprenticeComponent : Component
{
    [DataField]
    public string StatusIcon = "ApprenticeFaction";
}
