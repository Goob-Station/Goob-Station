using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wizard.CurseOfByond;

[RegisterComponent, NetworkedComponent]
public sealed partial class CurseOfByondComponent : Component
{
    [DataField]
    public string CurseOfByondAlertKey = "CurseOfByond";
}
