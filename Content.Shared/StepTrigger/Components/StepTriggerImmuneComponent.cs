using Content.Shared.StepTrigger.Prototypes;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.StepTrigger.Components;
/// <summary>
///     Goobstation: This component marks an entity as being immune to all step triggers.
///     For example, a Harpy being so low density, that they don't set off landmines.
/// </summary>
/// <remarks>
///     This is the "Earliest Possible Exit" method, and therefore isn't possible to un-cancel.
///     It will prevent ALL step trigger events from firing. Therefore there may sometimes be unintended consequences to this.
///     Consider using a subscription to StepTriggerAttemptEvent if you wish to be more selective.
/// </remarks>
[RegisterComponent, NetworkedComponent]
public sealed partial class StepTriggerImmuneComponent : Component { }
[Access(typeof(StepTriggerSystem))]
public sealed partial class StepTriggerImmuneComponent : Component
{
    /// <summary>
    ///     WhiteList of immunity step triggers.
    /// </summary>
    [DataField]
    public StepTriggerGroup? Whitelist;
}
