namespace Content.Goobstation.Server.Heretic.Abilities.HereticFlames;

[RegisterComponent]
public sealed partial class HereticFlamesComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public float UpdateTimer = 0f;

    [ViewVariables(VVAccess.ReadOnly)]
    public float LifetimeTimer = 0f;

    [DataField]
    public float UpdateDuration = .2f;

    [DataField]
    public float LifetimeDuration = 60f;
}
