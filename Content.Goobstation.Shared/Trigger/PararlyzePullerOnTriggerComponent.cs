using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Trigger;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ParalyzePullerOnTriggerComponent : BaseXOnTriggerComponent
{
    [DataField]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(5);
}
