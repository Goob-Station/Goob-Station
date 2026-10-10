using Robust.Shared.Map;

namespace Content.Shared._Shitcode.Wizard.FistFight;

[RegisterComponent]
public sealed partial class WizardFistFightSpectatorComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityCoordinates Home;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool WasPacified;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool WasGodmoded;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? PacifiedUntil;
}
