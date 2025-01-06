namespace Content.Server._Goobstation.ImpendingDoom;

[RegisterComponent, Access(typeof(ImpendingDoomSystem))]
public sealed partial class DoomedComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan DoomAt;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan AppliedAt;


}
