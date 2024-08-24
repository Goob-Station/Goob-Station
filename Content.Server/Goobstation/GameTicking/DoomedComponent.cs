namespace Content.Server.Goobstation.GameTicking;

[RegisterComponent, Access(typeof(ImpendingDoomSystem))]
public sealed partial class DoomedComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Applied;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan AppliedAt;
}
