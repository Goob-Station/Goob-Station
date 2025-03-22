using Robust.Shared.GameStates;
namespace Content.Shared._Goobstation.Wizard.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CurseOfByondComponent : Component
{
    [DataField]
    public string CurseOfByondAlertKey = "CurseOfByond";
}
